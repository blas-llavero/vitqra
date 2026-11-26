/* SPDX-License-Identifier: Apache-2.0
 * VITQRA native speech adapter; build against the reviewed sherpa-onnx C API.
 * No network access or file output is implemented by this adapter.
 */
#include <stdint.h>
#include <stddef.h>
#include <string.h>
#include "sherpa-onnx/c-api/c-api.h"

#if defined(_WIN32)
#define VEXPORT __declspec(dllexport)
#else
#define VEXPORT __attribute__((visibility("default")))
#endif

VEXPORT int32_t vitqra_speech_abi(void) { return 1; }

/* All strings are UTF-8 NUL-terminated. Caller owns input/output buffers.
 * Return positive result length, zero for empty transcription, negative for error.
 */
VEXPORT int32_t vitqra_transcribe(const char *encoder, const char *decoder,
    const char *tokens, const char *language, const float *pcm,
    int32_t count, int32_t rate, char *output, int32_t capacity) {
    if (!encoder || !decoder || !tokens || !language || !pcm || !output ||
        count <= 0 || rate < 8000 || rate > 96000 ||
        count > 21 * rate || capacity <= 1) return -1;
    output[0] = 0;
    SherpaOnnxOfflineRecognizerConfig cfg = {0};
    cfg.decoding_method = "greedy_search";
    cfg.model_config.provider = "cpu";
    cfg.model_config.num_threads = 2;
    cfg.model_config.debug = 0;
    cfg.model_config.tokens = tokens;
    cfg.model_config.whisper.encoder = encoder;
    cfg.model_config.whisper.decoder = decoder;
    cfg.model_config.whisper.language = language;
    cfg.model_config.whisper.task = "transcribe";
    const SherpaOnnxOfflineRecognizer *recognizer = SherpaOnnxCreateOfflineRecognizer(&cfg);
    if (!recognizer) return -2;
    int32_t rc = -2;
    const SherpaOnnxOfflineStream *stream = SherpaOnnxCreateOfflineStream(recognizer);
    if (stream) {
        SherpaOnnxAcceptWaveformOffline(stream, rate, pcm, count);
        SherpaOnnxDecodeOfflineStream(recognizer, stream);
        const SherpaOnnxOfflineRecognizerResult *result = SherpaOnnxGetOfflineStreamResult(stream);
        if (result) {
            if (result->text) {
                size_t n = strlen(result->text);
                if (n < (size_t)capacity) {
                    memcpy(output,result->text,n+1);
                    rc = (int32_t)n;
                } else rc = -3;
            }
            SherpaOnnxDestroyOfflineRecognizerResult(result);
        }
        SherpaOnnxDestroyOfflineStream(stream);
    }
    SherpaOnnxDestroyOfflineRecognizer(recognizer);
    return rc;
}

/* Lexicon-based VITS only. No arbitrary voice cloning or phonemizer fallback.
 * Return sample count. The supplied PCM buffer is owned by the caller.
 */
VEXPORT int32_t vitqra_synthesize(const char *model,const char *lexicon,
    const char *tokens,const char *text,int32_t speaker,float speed,
    float *output,int32_t capacity,int32_t *sample_rate) {
    if (!model || !lexicon || !tokens || !text || !output || !sample_rate ||
        capacity<=0 || speaker<0 || !(speed>=0.75f && speed<=1.25f)) return -1;
    *sample_rate=0;
    SherpaOnnxOfflineTtsConfig cfg={0};
    cfg.model.vits.model=model;
    cfg.model.vits.lexicon=lexicon;
    cfg.model.vits.tokens=tokens;
    cfg.model.vits.noise_scale=0.667f;
    cfg.model.vits.noise_scale_w=0.8f;
    cfg.model.vits.length_scale=1.0f;
    cfg.model.num_threads=2;
    cfg.model.provider="cpu";
    cfg.model.debug=0;
    const SherpaOnnxOfflineTts *tts=SherpaOnnxCreateOfflineTts(&cfg);
    if(!tts) return -2;
    SherpaOnnxGenerationConfig gen={0};
    gen.silence_scale=0.2f;
    gen.sid=speaker;
    gen.speed=speed;
    const SherpaOnnxGeneratedAudio *audio=SherpaOnnxOfflineTtsGenerateWithConfig(tts,text,&gen,NULL,NULL);
    int32_t rc=-2;
    if(audio) {
        if(audio->samples && audio->n>0 && audio->n<=capacity &&
           audio->sample_rate>=8000 && audio->sample_rate<=48000 &&
           audio->n<=60*audio->sample_rate) {
            memcpy(output,audio->samples,(size_t)audio->n*sizeof(float));
            *sample_rate=audio->sample_rate;
            rc=audio->n;
        } else rc=-3;
        SherpaOnnxDestroyOfflineTtsGeneratedAudio(audio);
    }
    SherpaOnnxDestroyOfflineTts(tts);
    return rc;
}
