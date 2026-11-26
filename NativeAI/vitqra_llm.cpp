// SPDX-License-Identifier: Apache-2.0
// CPU-only local inference adapter. Uses the public llama.cpp API; no network,
// shell commands, tool execution, automatic model download or persistent prompts.
#include "llama.h"
#include <atomic>
#include <algorithm>
#include <cstdint>
#include <cstring>
#include <memory>
#include <mutex>
#include <new>
#include <string>
#include <vector>
#if defined(_WIN32)
#define VEXPORT extern "C" __declspec(dllexport)
#else
#define VEXPORT extern "C" __attribute__((visibility("default")))
#endif
namespace {
struct Job { std::atomic<bool> canceled{false}; };
std::once_flag backend_once;
std::mutex inference_gate;
bool abort_decode(void * p) { return static_cast<Job *>(p)->canceled.load(); }
bool load_progress(float, void * p) { return !abort_decode(p); }
void discard_log(enum ggml_log_level, const char *, void *) {}
constexpr int32_t context_size=2048, max_generated=384, max_prompt_bytes=16384;
}
VEXPORT int32_t vitqra_llm_abi() { return 1; }
VEXPORT void * vitqra_llm_create() { return new(std::nothrow) Job(); }
VEXPORT void vitqra_llm_cancel(void * job) { if(job)static_cast<Job *>(job)->canceled.store(true); }
// Call destroy only after generate has returned.
VEXPORT void vitqra_llm_destroy(void * job) { delete static_cast<Job *>(job); }

// Positive result: UTF-8 byte count. Negative: error/canceled/context overflow.
// Status 1 in truncated means the answer reached a generation or output bound.
VEXPORT int32_t vitqra_llm_generate(void * handle,const char * model_path,
    const char * system,const char * prior_user,const char * prior_assistant,
    const char * user,char * output,int32_t capacity,int32_t * truncated) {
    if(!handle||!model_path||!system||!prior_user||!prior_assistant||!user||
       !output||!truncated||capacity<2||capacity>32768)return -1;
    output[0]=0;*truncated=0;
    auto * job=static_cast<Job *>(handle);
    try {
        std::lock_guard<std::mutex> lock(inference_gate);
        if(abort_decode(job))return -2;
        std::call_once(backend_once,[]{llama_log_set(discard_log,nullptr);llama_backend_init();});
        auto mp=llama_model_default_params();
        mp.n_gpu_layers=0;mp.check_tensors=true;
        mp.progress_callback=load_progress;mp.progress_callback_user_data=job;
        std::unique_ptr<llama_model,decltype(&llama_model_free)> model(
            llama_model_load_from_file(model_path,mp),llama_model_free);
        if(!model||abort_decode(job))return -2;
        if(llama_model_has_encoder(model.get())||!llama_model_has_decoder(model.get()))return -3;
        const auto * vocab=llama_model_get_vocab(model.get());
        const char * tmpl=llama_model_chat_template(model.get(),nullptr);
        if(!vocab||!tmpl||!*tmpl)return -3;
        std::vector<llama_chat_message> chat{{"system",system}};
        if(*prior_user&&*prior_assistant){
            chat.push_back({"user",prior_user});chat.push_back({"assistant",prior_assistant});
        }
        chat.push_back({"user",user});
        int32_t size=llama_chat_apply_template(tmpl,chat.data(),chat.size(),true,nullptr,0);
        if(size<=0||size>max_prompt_bytes)return -4;
        std::vector<char> prompt(size+1);
        if(llama_chat_apply_template(tmpl,chat.data(),chat.size(),true,prompt.data(),prompt.size())!=size)return -4;
        int32_t count=-llama_tokenize(vocab,prompt.data(),size,nullptr,0,true,true);
        if(count<=0||count>context_size-max_generated)return -4;
        std::vector<llama_token> tokens(count);
        if(llama_tokenize(vocab,prompt.data(),size,tokens.data(),count,true,true)!=count)return -4;
        auto cp=llama_context_default_params();
        cp.n_ctx=context_size;cp.n_batch=context_size;cp.n_ubatch=128;
        cp.n_threads=2;cp.n_threads_batch=2;cp.offload_kqv=false;cp.op_offload=false;
        cp.abort_callback=abort_decode;cp.abort_callback_data=job;
        std::unique_ptr<llama_context,decltype(&llama_free)> ctx(llama_init_from_model(model.get(),cp),llama_free);
        if(!ctx||abort_decode(job))return -2;
        if(count+max_generated>static_cast<int32_t>(llama_n_ctx(ctx.get())))return -4;
        std::unique_ptr<llama_sampler,decltype(&llama_sampler_free)> sampler(llama_sampler_init_greedy(),llama_sampler_free);
        if(!sampler)return -3;
        // Legacy single-sequence batch API is available at the pinned revision.
        if(llama_decode(ctx.get(),llama_batch_get_one(tokens.data(),count))!=0)return -5;
        std::string result;
        bool ended=false;
        for(int i=0;i<max_generated;i++){
            if(abort_decode(job))return -2;
            llama_token token=llama_sampler_sample(sampler.get(),ctx.get(),-1);
            if(llama_vocab_is_eog(vocab,token)){ended=true;break;}
            char piece[1024];
            int32_t n=llama_token_to_piece(vocab,token,piece,sizeof(piece),0,false);
            if(n<0||n>static_cast<int32_t>(sizeof(piece)))return -5;
            if(result.size()+n>=static_cast<size_t>(capacity)){*truncated=1;break;}
            result.append(piece,n);
            if(i+1<max_generated&&llama_decode(ctx.get(),llama_batch_get_one(&token,1))!=0)return -5;
        }
        if(abort_decode(job))return -2;
        if(!ended)*truncated=1;
        if(result.empty())return -5;
        // The managed caller validates UTF-8 and never displays an invalid byte sequence.
        std::memcpy(output,result.data(),result.size());output[result.size()]=0;
        return static_cast<int32_t>(result.size());
    } catch(...) { return -5; }
}
