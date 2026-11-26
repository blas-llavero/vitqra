// SPDX-License-Identifier: Apache-2.0
package com.blasllavero.vitqra.account;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.net.Uri;
import android.security.keystore.KeyGenParameterSpec;
import android.security.keystore.KeyProperties;
import android.util.Base64;
import org.json.JSONArray;
import org.json.JSONObject;
import java.io.*;
import java.net.*;
import java.nio.charset.StandardCharsets;
import java.security.*;
import java.security.spec.RSAPublicKeySpec;
import java.math.BigInteger;
import java.util.*;
import java.util.concurrent.*;
import javax.crypto.*;
import javax.crypto.spec.GCMParameterSpec;

/** Development account connector. No inference endpoint or token export. */
public final class VitqraChatAccount {
    private static final String ISSUER="https://auth.openai.com";
    private static final String RESOURCE="https://api.openai.com/v1";
    private static final String SCOPES="openid profile email offline_access resource.invoke chatgpt.tokens.use.direct";
    private static final String PREF="vitqra_chat_account_v1", ALIAS="vitqra_chat_account_key_v1";
    private static final SecureRandom RNG=new SecureRandom();
    private static final ExecutorService IO=Executors.newSingleThreadExecutor();
    private static final Object LOCK=new Object();
    private static volatile int status=0, generation=0;
    private static volatile long expiresAt=0;
    private static volatile ServerSocket listener;
    private static boolean initialized=false, busy=false;
    // 0 disconnected, 1 busy, 2 plan scope granted, 3 identity only,
    // 4 failed, 5 expired, 7 local sign-out done; remote revocation unconfirmed.
    private VitqraChatAccount(){}
    private static SharedPreferences prefs(Context c){return c.getSharedPreferences(PREF,Context.MODE_PRIVATE);}
    private static byte[] bytes(String s){return s.getBytes(StandardCharsets.UTF_8);}
    private static String enc(String s)throws Exception{return URLEncoder.encode(s,"UTF-8");}
    private static String b64(byte[] b){return Base64.encodeToString(b,Base64.NO_WRAP|Base64.NO_PADDING|Base64.URL_SAFE);}
    private static byte[] decode(String s){return Base64.decode(s,Base64.NO_WRAP|Base64.NO_PADDING|Base64.URL_SAFE);}
    private static String random(int n){byte[] b=new byte[n];RNG.nextBytes(b);return b64(b);}
    private static long now(){return System.currentTimeMillis()/1000;}
    private static boolean same(String a,String b){return a!=null&&b!=null&&MessageDigest.isEqual(bytes(a),bytes(b));}
    private static boolean validClient(String s){return s!=null&&s.matches("[A-Za-z0-9._~-]{1,200}")&&!s.equals("dynamic_agent_client");}
    private static String host(Context c)throws Exception{
        SharedPreferences p=prefs(c);
        String host=p.getString("host",null);
        if(host==null){
            host="urn:uuid:"+UUID.randomUUID();
            if(!p.edit().putString("host",host).commit())throw new IOException();
        }
        return host;
    }
    private static Key key()throws Exception{
        KeyStore ks=KeyStore.getInstance("AndroidKeyStore");ks.load(null);
        if(!ks.containsAlias(ALIAS)){
            KeyGenerator kg=KeyGenerator.getInstance(KeyProperties.KEY_ALGORITHM_AES,"AndroidKeyStore");
            kg.init(new KeyGenParameterSpec.Builder(ALIAS,KeyProperties.PURPOSE_ENCRYPT|KeyProperties.PURPOSE_DECRYPT)
                .setBlockModes(KeyProperties.BLOCK_MODE_GCM).setEncryptionPaddings(KeyProperties.ENCRYPTION_PADDING_NONE)
                .setKeySize(256).build());kg.generateKey();
        }
        return ks.getKey(ALIAS,null);
    }
    private static JSONObject load(Context c)throws Exception{
        String saved=prefs(c).getString("sealed",null);
        if(saved==null)return new JSONObject();
        if(saved.length()>200000)throw new IOException();
        byte[] all=Base64.decode(saved,Base64.NO_WRAP);
        if(all.length<29)throw new IOException();
        Cipher cipher=Cipher.getInstance("AES/GCM/NoPadding");
        cipher.init(Cipher.DECRYPT_MODE,key(),new GCMParameterSpec(128,Arrays.copyOfRange(all,0,12)));
        cipher.updateAAD(bytes("VITQRA account v1"));
        return new JSONObject(new String(cipher.doFinal(Arrays.copyOfRange(all,12,all.length)),StandardCharsets.UTF_8));
    }
    private static void save(Context c,JSONObject record)throws Exception{
        byte[] plain=bytes(record.toString());
        if(plain.length>100000)throw new IOException();
        Cipher cipher=Cipher.getInstance("AES/GCM/NoPadding");cipher.init(Cipher.ENCRYPT_MODE,key());
        cipher.updateAAD(bytes("VITQRA account v1"));
        byte[] iv=cipher.getIV(),encrypted;
        if(iv.length!=12)throw new IOException();
        try{encrypted=cipher.doFinal(plain);}finally{Arrays.fill(plain,(byte)0);}
        byte[] all=new byte[iv.length+encrypted.length];System.arraycopy(iv,0,all,0,iv.length);
        System.arraycopy(encrypted,0,all,iv.length,encrypted.length);
        if(!prefs(c).edit().putString("sealed",Base64.encodeToString(all,Base64.NO_WRAP)).commit())throw new IOException();
    }
    private static String form(String... pairs)throws Exception{
        StringBuilder b=new StringBuilder();
        for(int i=0;i<pairs.length;i+=2){if(i>0)b.append('&');b.append(enc(pairs[i])).append('=').append(enc(pairs[i+1]));}
        return b.toString();
    }
    private static JSONObject request(String url,String body,boolean emptyOk)throws Exception{
        URL u=new URL(url);
        if(!"https".equals(u.getProtocol())||!"auth.openai.com".equals(u.getHost())||
            u.getUserInfo()!=null||u.getPort()!=-1||u.getQuery()!=null||u.getRef()!=null)throw new IOException();
        HttpURLConnection c=(HttpURLConnection)u.openConnection();
        c.setInstanceFollowRedirects(false);c.setConnectTimeout(12000);c.setReadTimeout(15000);
        c.setUseCaches(false);c.setRequestProperty("Accept","application/json");
        try{
            if(body!=null){
                c.setRequestMethod("POST");c.setDoOutput(true);
                c.setRequestProperty("Content-Type","application/x-www-form-urlencoded");
                byte[] b=bytes(body);c.setFixedLengthStreamingMode(b.length);
                try(OutputStream out=c.getOutputStream()){out.write(b);}finally{Arrays.fill(b,(byte)0);}
            }
            int code=c.getResponseCode();if(code<200||code>=300)throw new IOException();
            try(InputStream in=c.getInputStream();ByteArrayOutputStream out=new ByteArrayOutputStream()){
                byte[] b=new byte[4096];int n;
                while((n=in.read(b))!=-1){if(out.size()+n>262144)throw new IOException();out.write(b,0,n);}
                return emptyOk?new JSONObject():new JSONObject(out.toString("UTF-8"));
            }
        }finally{c.disconnect();}
    }
    private static JSONObject verifyIdentity(String jwt,String client,String nonce)throws Exception{
        if(jwt.length()>32768)throw new IOException();
        String[] parts=jwt.split("\\.",-1);if(parts.length!=3)throw new IOException();
        JSONObject h=new JSONObject(new String(decode(parts[0]),StandardCharsets.UTF_8));
        if(!"RS256".equals(h.optString("alg"))||h.optString("kid").isEmpty())throw new IOException();
        JSONArray keys=request(ISSUER+"/.well-known/jwks.json",null,false).getJSONArray("keys");
        boolean ok=false;
        for(int i=0;i<keys.length();i++){
            JSONObject k=keys.getJSONObject(i);
            if(!h.getString("kid").equals(k.optString("kid"))||!"RSA".equals(k.optString("kty")))continue;
            BigInteger n=new BigInteger(1,decode(k.getString("n"))),e=new BigInteger(1,decode(k.getString("e")));
            if(n.bitLength()<2048||n.bitLength()>8192)throw new IOException();
            Signature sig=Signature.getInstance("SHA256withRSA");
            sig.initVerify(KeyFactory.getInstance("RSA").generatePublic(new RSAPublicKeySpec(n,e)));
            sig.update(bytes(parts[0]+"."+parts[1]));ok=sig.verify(decode(parts[2]));break;
        }
        if(!ok)throw new IOException();
        JSONObject claims=new JSONObject(new String(decode(parts[1]),StandardCharsets.UTF_8));
        Object aud=claims.opt("aud");boolean match=client.equals(aud);
        if(aud instanceof JSONArray){
            JSONArray a=(JSONArray)aud;
            for(int i=0;i<a.length();i++)if(client.equals(a.optString(i)))match=true;
            if(a.length()>1&&!client.equals(claims.optString("azp")))match=false;
        }
        if(claims.has("azp")&&!client.equals(claims.optString("azp")))match=false;
        if(!ISSUER.equals(claims.optString("iss"))||!match||!same(nonce,claims.optString("nonce"))||
           claims.optString("sub").isEmpty()||claims.optLong("exp",0)<=now()||
           claims.optLong("iat",Long.MAX_VALUE)>now()+60||claims.optLong("nbf",0)>now()+60)throw new IOException();
        return claims;
    }
    private static boolean sharing(JSONObject rec){
        return Arrays.asList(rec.optString("scope","").split("\\s+")).contains("chatgpt.tokens.use.direct")&&
            Arrays.asList(rec.optString("scope","").split("\\s+")).contains("resource.invoke")&&
            !rec.optString("access").isEmpty();
    }
    private static int state(JSONObject rec){
        expiresAt=rec.optLong("expires",0);
        if(rec.optString("identity").isEmpty())return 0;
        if(expiresAt<=now())return 5;
        return sharing(rec)?2:3;
    }
    public static int getStatus(){int s=status;return (s==2||s==3)&&expiresAt<=now()?5:s;}
    public static void initialize(Activity a){
        synchronized(LOCK){
            if(initialized)return;
            initialized=true;busy=true;status=1;
        }
        Context c=a.getApplicationContext();
        IO.execute(()->{synchronized(LOCK){try{status=state(load(c));}catch(Exception e){status=4;}finally{busy=false;}}});
    }
    private static String line(InputStream in,int max)throws Exception{
        ByteArrayOutputStream b=new ByteArrayOutputStream();
        while(b.size()<=max){
            int ch=in.read();if(ch<0)throw new EOFException();
            if(ch==13){if(in.read()!=10)throw new IOException();return b.toString("US-ASCII");}
            if(ch<32||ch>126)throw new IOException();
            b.write(ch);
        }
        throw new IOException();
    }
    private static Map<String,String> callback(Socket socket,int port,String state)throws Exception{
        socket.setSoTimeout(5000);
        InputStream in=new BufferedInputStream(socket.getInputStream());
        String[] req=line(in,8192).split(" ",-1);
        if(req.length!=3||!"GET".equals(req[0])||!"HTTP/1.1".equals(req[2])||!req[1].startsWith("/auth/callback?"))throw new IOException();
        String host=null;int sum=0;
        for(int i=0;i<40;i++){
            String h=line(in,2048);sum+=h.length();if(sum>8192)throw new IOException();
            if(h.isEmpty())break;
            if(i==39)throw new IOException();
            int colon=h.indexOf(':');if(colon<=0)throw new IOException();
            if(h.substring(0,colon).equalsIgnoreCase("Host")){if(host!=null)throw new IOException();host=h.substring(colon+1).trim();}
        }
        if(!("127.0.0.1:"+port).equals(host))throw new IOException();
        Map<String,String> values=new HashMap<>();
        for(String part:req[1].substring("/auth/callback?".length()).split("&")){
            int at=part.indexOf('=');if(at<1)throw new IOException();
            String k=URLDecoder.decode(part.substring(0,at),"UTF-8"),v=URLDecoder.decode(part.substring(at+1),"UTF-8");
            if(values.put(k,v)!=null)throw new IOException();
        }
        if(!same(state,values.get("state")))throw new IOException();
        return values;
    }
    private static void browserReply(Socket socket)throws IOException{
        byte[] b=bytes("Authorization received. Return to VITQRA to check the result.");
        OutputStream out=socket.getOutputStream();
        out.write(bytes("HTTP/1.1 200 OK\r\nContent-Type: text/plain; charset=UTF-8\r\nCache-Control: no-store\r\nReferrer-Policy: no-referrer\r\nX-Content-Type-Options: nosniff\r\nContent-Security-Policy: default-src 'none'\r\nConnection: close\r\nContent-Length: "+b.length+"\r\n\r\n"));
        out.write(b);out.flush();
    }
    public static void signIn(Activity activity){
        final int attempt;
        synchronized(LOCK){if(busy)return;busy=true;status=1;attempt=++generation;}
        Context c=activity.getApplicationContext();
        IO.execute(()->{
            int outcome=4;
            try(ServerSocket server=new ServerSocket()){
                JSONObject previous=load(c);
                String priorClient=previous.optString("client"),priorSubject=previous.optString("subject");
                boolean first=!validClient(priorClient);
                server.bind(new InetSocketAddress(InetAddress.getByName("127.0.0.1"),0));
                server.setSoTimeout(120000);
                synchronized(LOCK){if(attempt!=generation)throw new IOException();listener=server;}
                int port=server.getLocalPort();
                String redirect="http://127.0.0.1:"+port+"/auth/callback";
                String state=random(32),nonce=random(32),verifier=random(48);
                String challenge=b64(MessageDigest.getInstance("SHA-256").digest(bytes(verifier)));
                String query=form("client_id",first?"dynamic_agent_client":priorClient,"response_type","code",
                    "redirect_uri",redirect,"scope",SCOPES,"resource",RESOURCE,"state",state,"nonce",nonce,
                    "code_challenge_method","S256","code_challenge",challenge,"ext_agent_host_id",host(c));
                if(first)query+="&"+form("agent_name_hint","VITQRA");
                else {
                    String old=previous.optString("identity");
                    if(!old.isEmpty())query+="&"+form("id_token_hint",old);
                    query+="&"+form("prompt","consent");
                }
                final String address=ISSUER+"/api/accounts/authorize?"+query;
                activity.runOnUiThread(()->{
                    if(attempt!=generation)return;
                    try{activity.startActivity(new Intent(Intent.ACTION_VIEW,Uri.parse(address)));}
                    catch(Exception ignored){closeListener();}
                });
                Map<String,String> values;
                try(Socket socket=server.accept()){values=callback(socket,port,state);browserReply(socket);}
                if(values.containsKey("error"))throw new IOException();
                String code=values.get("code"),client=values.get("client_id");
                if(code==null||code.isEmpty()||code.length()>4096)throw new IOException();
                if(first&&!validClient(client))throw new IOException();
                if(!first){if(client!=null&&!priorClient.equals(client))throw new IOException();client=priorClient;}
                synchronized(LOCK){
                    if(attempt!=generation)throw new IOException();
                    if(first){JSONObject reg=new JSONObject();reg.put("client",client);save(c,reg);}
                }
                JSONObject tokens=request(ISSUER+"/api/accounts/oauth/token",
                    form("grant_type","authorization_code","client_id",client,"code",code,"code_verifier",verifier,
                    "redirect_uri",redirect,"resource",RESOURCE),false);
                String jwt=tokens.getString("id_token");
                JSONObject identity=verifyIdentity(jwt,client,nonce);
                String subject=identity.getString("sub");
                if(!priorSubject.isEmpty()&&!same(priorSubject,subject))throw new IOException();
                long seconds=tokens.optLong("expires_in",0);
                if(seconds<=0||seconds>31536000L)throw new IOException();
                JSONObject record=new JSONObject();
                record.put("client",client);record.put("subject",subject);record.put("identity",jwt);
                record.put("scope",tokens.optString("scope",""));record.put("expires",now()+seconds);
                if("Bearer".equalsIgnoreCase(tokens.optString("token_type"))){
                    record.put("access",tokens.optString("access_token",""));
                    record.put("refresh",tokens.optString("refresh_token",""));
                }
                synchronized(LOCK){if(attempt!=generation)throw new IOException();save(c,record);outcome=state(record);}
            }catch(Exception ignored){}
            finally{
                synchronized(LOCK){listener=null;busy=false;if(attempt==generation)status=outcome;else try{status=state(load(c));}catch(Exception e){status=4;}}
            }
        });
    }
    private static void closeListener(){ServerSocket s=listener;if(s!=null)try{s.close();}catch(IOException ignored){}}
    public static void cancel(){synchronized(LOCK){if(busy&&listener!=null){generation++;closeListener();}}}
    public static void signOut(Activity activity){
        synchronized(LOCK){if(busy)return;busy=true;status=1;}
        Context c=activity.getApplicationContext();
        IO.execute(()->{
            int outcome=4;
            try{
                JSONObject old=load(c),registration=new JSONObject();
                registration.put("client",old.optString("client"));registration.put("subject",old.optString("subject"));
                // Clear local credentials first; revocation is best effort.
                save(c,registration);expiresAt=0;
                outcome=0;
                String refresh=old.optString("refresh");
                if(!refresh.isEmpty()){
                    try{
                        String url=request(ISSUER+"/.well-known/openid-configuration",null,false).getString("revocation_endpoint");
                        request(url,form("token",refresh,"token_type_hint","refresh_token","client_id",old.getString("client")),true);
                    }catch(Exception ignored){outcome=7;}
                }else if(!old.optString("access").isEmpty())outcome=7;
            }catch(Exception ignored){
                // An unreadable encrypted record can be cleared without exposing it.
                if(prefs(c).edit().remove("sealed").commit())outcome=7;
            }
            synchronized(LOCK){status=outcome;busy=false;}
        });
    }
}
