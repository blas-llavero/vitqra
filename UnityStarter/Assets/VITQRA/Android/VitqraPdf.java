// SPDX-License-Identifier: Apache-2.0
package com.blasllavero.vitqra.pdf;

import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.graphics.Typeface;
import android.graphics.pdf.PdfDocument;
import android.graphics.pdf.PdfRenderer;
import android.os.ParcelFileDescriptor;
import android.text.Layout;
import android.text.StaticLayout;
import android.text.TextPaint;
import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.IOException;
import java.io.OutputStream;

/** Local generated reports only. No HTML, network, scripts or external PDF access. */
public final class VitqraPdf {
    private static final int WIDTH=595, HEIGHT=842, MARGIN=42, BODY_TOP=102, BODY_BOTTOM=744;
    private static final int MAX_PAGES=48, MAX_PDF=8*1024*1024, MAX_PNG=4*1024*1024;
    private static final class Bounded extends OutputStream {
        final ByteArrayOutputStream data=new ByteArrayOutputStream();
        final int limit;
        Bounded(int limit){this.limit=limit;}
        @Override public void write(int b) throws IOException {
            if(data.size()>=limit)throw new IOException("Output limit");
            data.write(b);
        }
        @Override public void write(byte[] b,int off,int len) throws IOException {
            if(len<0 || len>limit-data.size())throw new IOException("Output limit");
            data.write(b,off,len);
        }
    }
    private static TextPaint paint(float size,int color,boolean bold){
        TextPaint p=new TextPaint(Paint.ANTI_ALIAS_FLAG);
        p.setTextSize(size);p.setColor(color);
        p.setTypeface(Typeface.create("sans-serif",bold?Typeface.BOLD:Typeface.NORMAL));
        return p;
    }
    private static StaticLayout layout(String text,TextPaint p){
        return StaticLayout.Builder.obtain(text,0,text.length(),p,WIDTH-2*MARGIN)
            .setAlignment(Layout.Alignment.ALIGN_NORMAL).setIncludePad(true).setLineSpacing(2f,1f).build();
    }
    private static void draw(Canvas canvas,StaticLayout layout,float x,float y){
        int save=canvas.save();canvas.translate(x,y);layout.draw(canvas);canvas.restoreToCount(save);
    }
    public static byte[] generate(String title,String body,String warning) throws IOException {
        if(title==null||title.length()>300||body==null||body.length()==0||body.length()>40000||
           warning==null||warning.length()>600)throw new IOException("Invalid report");
        TextPaint normal=paint(11f,Color.rgb(30,48,60),false);
        TextPaint heading=paint(16f,Color.rgb(18,87,97),true);
        TextPaint footer=paint(9f,Color.rgb(85,62,30),false);
        StaticLayout titleLayout=layout(title,heading), warningLayout=layout(warning,footer);
        if(titleLayout.getHeight()>52||warningLayout.getHeight()>62)throw new IOException("Header or footer exceeds page");
        StaticLayout content=layout(body,normal);
        int line=0,pageNumber=0;
        Bounded bytes=new Bounded(MAX_PDF);
        try(PdfDocument pdf=new PdfDocument()){
            while(line<content.getLineCount()){
                if(++pageNumber>MAX_PAGES)throw new IOException("Page limit");
                int end=line+1,top=content.getLineTop(line);
                while(end<content.getLineCount()&&content.getLineTop(end+1)-top<=BODY_BOTTOM-BODY_TOP)end++;
                int height=content.getLineTop(end)-top;
                if(height<=0||height>BODY_BOTTOM-BODY_TOP)throw new IOException("Line exceeds page");
                PdfDocument.Page page=pdf.startPage(new PdfDocument.PageInfo.Builder(WIDTH,HEIGHT,pageNumber).create());
                try{
                    Canvas canvas=page.getCanvas();
                    canvas.drawColor(Color.WHITE);
                    canvas.drawText("VITQRA  |  Science without barriers",MARGIN,31,heading);
                    draw(canvas,titleLayout,MARGIN,42);
                    Paint rule=paint(1,Color.rgb(37,140,138),false);
                    canvas.drawLine(MARGIN,94,WIDTH-MARGIN,94,rule);
                    int save=canvas.save();
                    canvas.clipRect(MARGIN,BODY_TOP,WIDTH-MARGIN,BODY_TOP+height);
                    canvas.translate(MARGIN,BODY_TOP-top);content.draw(canvas);
                    canvas.restoreToCount(save);
                    canvas.drawLine(MARGIN,752,WIDTH-MARGIN,752,rule);
                    draw(canvas,warningLayout,MARGIN,758);
                    canvas.drawText(Integer.toString(pageNumber),WIDTH-MARGIN-22,830,footer);
                }finally{pdf.finishPage(page);}
                line=end;
            }
            pdf.writeTo(bytes);
        }
        return bytes.data.toByteArray();
    }
    // The managed caller passes a freshly written private copy of a verified,
    // locally generated PDF. Never call this with a downloaded file or URI.
    public static int pageCount(String path) throws IOException {
        try(ParcelFileDescriptor fd=ParcelFileDescriptor.open(new File(path),ParcelFileDescriptor.MODE_READ_ONLY)){
            // PdfRenderer owns its descriptor; duplicate so lifetimes are independent.
            try(PdfRenderer renderer=new PdfRenderer(ParcelFileDescriptor.dup(fd.getFileDescriptor()))){
                int n=renderer.getPageCount();
                if(n<1||n>MAX_PAGES)throw new IOException("Page limit");
                return n;
            }
        }
    }
    public static byte[] render(String path,int index) throws IOException {
        try(ParcelFileDescriptor fd=ParcelFileDescriptor.open(new File(path),ParcelFileDescriptor.MODE_READ_ONLY);
            PdfRenderer renderer=new PdfRenderer(ParcelFileDescriptor.dup(fd.getFileDescriptor()))){
            int n=renderer.getPageCount();
            if(n<1||n>MAX_PAGES||index<0||index>=n)throw new IOException("Invalid page");
            try(PdfRenderer.Page page=renderer.openPage(index)){
                if(page.getWidth()!=WIDTH||page.getHeight()!=HEIGHT)throw new IOException("Invalid page dimensions");
                Bitmap bitmap=Bitmap.createBitmap(WIDTH*2,HEIGHT*2,Bitmap.Config.ARGB_8888);
                try{
                    bitmap.eraseColor(Color.WHITE);
                    page.render(bitmap,null,null,PdfRenderer.Page.RENDER_MODE_FOR_DISPLAY);
                    ByteArrayOutputStream png=new ByteArrayOutputStream();
                    if(!bitmap.compress(Bitmap.CompressFormat.PNG,100,png)||png.size()>MAX_PNG)throw new IOException("Image limit");
                    return png.toByteArray();
                }finally{bitmap.recycle();}
            }
        }
    }
}
