///*
// * To change this license header, choose License Headers in Project Properties.
// * To change this template file, choose Tools | Templates
// * and open the template in the editor.
// */
//package automenta.vivisect.swing;
//
//import automenta.vivisect.Vis;
//import processing.core.PApplet;
//import processing.core.PFont;
//import processing.core.PGraphics;
//import processing.event.MouseEvent;
//
//import javax.swing.*;
//import java.awt.event.HierarchyEvent;
//import java.awt.event.HierarchyListener;
//
///**
// *
// * @author me
// */
//public class PCanvas extends PApplet implements HierarchyListener {
//
//    int mouseScroll = 0;
//
//    final Hnav hnav = new Hnav();
//
//    float zoom = 0.1f;
//    float scale = 1f;
//    float selection_distance = 10;
//    float FrameRate = 25f;
//
//    boolean lowQuality = false;
//
//    boolean predrawing = false;
//    float motionBlur = 0.0f;
//    private final Vis vis;
//
//    float camspeed = 20.0f;
//    float scrollcammult = 0.92f;
//    boolean keyToo = true;
//    float savepx = 0;
//    float savepy = 0;
//    int selID = 0;
//
//    float difx = 0;
//    float dify = 0;
//    int lastscr = 0;
//    boolean EnableZooming = true;
//    float scrollcamspeed = 1.1f;
//
//
//    /*public static PFont font12;
//    public static PFont font9;
//    public static PFont font15;*/
//
//    private PGraphics b;
//    private boolean renderEveryFrame = false/*, renderEveryInput = true*/;
//    private static PFont font12 = null;
//
//    public PCanvas() {
//        this(null);
//    }
//
//    public PCanvas(Vis vis) {
//        this(vis, 26);
//    }
//
//    public PCanvas(Vis vis, int frameRate) {
//        super();
//
//
//        this.FrameRate = frameRate;
//
//        init();
//
//
//        //hint(ENABLE_NATIVE_FONTS);
//        hint(ENABLE_OPTIMIZED_STROKE);
//
//
//        if ((vis == null) && (this instanceof Vis)) {
//            //for subclasses:
//            vis = (Vis)this;
//        }
//        this.vis = vis;
//
//        vis.init(this);
//
//    }
//
//    float MouseToWorldCoordX(final int x) {
//        return 1 / zoom * (x - difx - width / 2);
//    }
//
//    float MouseToWorldCoordY(final int y) {
//        return 1 / zoom * (y - dify - height / 2);
//    }
//
//    public float getPanX() {
//        return difx;
//    }
//    public float getPanY() {
//        return dify;
//    }
//
//    public void setPanX(float px) { difx = -px; }
//    public void setPanY(float py) { dify = -py; }
//
//    public float getCursorX() {
//        return MouseToWorldCoordX(mouseX);
//    }
//
//    public float getCursorY() {
//        return MouseToWorldCoordY(mouseY);
//    }
//
//    @Override
//    protected void resizeRenderer(int newWidth, int newHeight) {
//        if ((newWidth > 0) && (newHeight > 0)) {
//            super.resizeRenderer(newWidth, newHeight);
//            predrawing = false;
//        }
//    }
//
//    public void mouseScrolled() {
//        hnav.mouseScrolled();
//    }
//
//    @Override
//    public void keyPressed() {
//        hnav.keyPressed();
//    }
//
//    @Override
//    public void mouseMoved() {
//
//    }
//
//    @Override
//    public void mouseReleased() {
//        hnav.mouseReleased();
//        super.mouseReleased();
//    }
//
//    @Override
//    public void mouseDragged() {
//        hnav.mouseDragged();
//        super.mouseDragged();
//    }
//
//    @Override
//    public void mousePressed() {
//        hnav.mousePressed();
//        super.mousePressed();
//    }
//
//    public void setLowQuality(boolean lowQuality) {
//        this.lowQuality = lowQuality;
//    }
//
//    public boolean isPredrawing() {
//        return predrawing;
//    }
//
//    public synchronized void predraw() {
//        predrawing = true;
//
//        if ((b!=null) && (b.width != getWidth() || b.height != getHeight())) {
//            b.dispose();
//            b = null;
//        }
//        if (b == null) {
//            if ((getWidth() == 0) || (getHeight() == 0))
//                return;
//
//            b = createGraphics(getWidth(), getHeight(), JAVA2D);
//        }
//
//        b.beginDraw();
//
//        if (lowQuality)
//            b.noSmooth();
//        else
//            b.smooth();
//
//
//        b.strokeJoin(MITER);
//        b.strokeCap(PROJECT);
//
//        if (motionBlur > 0) {
//            b.fill(0, 0, 0, 255f * (1.0f - motionBlur));
//            b.rect(0, 0, getWidth(), getHeight());
//        } else {
//            b.background(0, 0, 0);//, 0.001f);
//        }
//
//
//        hnav.applyTransform(b);
//
//
//        vis.draw(b);
//
//        b.endDraw();
//
//        //if not autorendering, we need to invoke redraw manually here
//        if (!renderEveryFrame)
//            redraw();
//
//    }
//
//
////    public boolean renderEveryInput() {
//        //return renderEveryInput;
//    //}
//  //  public void renderEveryInput(boolean f) {
//      //  this.renderEveryInput = f;
//    //}
//
//    public boolean renderEveryFrame() {
//        return renderEveryFrame;
//    }
//    public void renderEveryFrame(boolean f) {
//        this.renderEveryFrame = f;
//    }
//
//    @Override
//    public void draw() {
//
//        if (predrawing && b!=null && b.width == getWidth() && b.height==getHeight())  {
//            try {
//                this.background(b);
//            }
//            catch (NullPointerException e) { }
//            predrawing = false;
//        }
//    }
//
//    public void setMotionBlur(float motionBlur) {
//        this.motionBlur = motionBlur;
//    }
//
//    public float getMotionBlur() {
//        return motionBlur;
//    }
//
//
//
//    @Override
//    public void mouseWheel(MouseEvent event) {
//        super.mouseWheel(event);
//        mouseScroll = -event.getCount();
//        mouseScrolled();
//    }
//
//    public PFont getDefaultFont() {
//        if (font12 == null) {
//            //font12 = createDefaultFont(12);
//            boolean smooth = false;
//            return this.createFont("Monofonto", 12, smooth, (char[]) null);
//        }
//
//        return font12;
//
//        /*font9 = createDefaultFont(9);
//        font15 = createDefaultFont(15);*/
//    }
//
//    @Override
//    public void setup() {
//
//        //size(500,500,P3D);
//        frameRate(FrameRate);
//
//
//        textFont(getDefaultFont());
//
//        if (isGL()) {
//            smooth();
//            System.out.println("Processing.org enabled OpenGL");
//        }
//
//        predraw();
//
//    }
//
//
//    @Override
//    public void addNotify() {
//        super.addNotify();
//        addHierarchyListener(this);
//    }
//
//    @Override
//    public void removeNotify() {
//        removeHierarchyListener(this);
//        super.removeNotify();
//    }
//
//    @Override
//    public void hierarchyChanged(HierarchyEvent e) {
//        if ((e.getChangeFlags() & HierarchyEvent.SHOWING_CHANGED) != 0) {
//            boolean showing = isShowing();
//            onShowing(showing);
//
//        }
//    }
//
//
//    protected void onShowing(boolean showing) {
//        vis.onVisible(showing);
//
//        if (showing) {
//            //restart loop? can this even happen
//            //throw new RuntimeException("if this happens, looping state should be restored here");
//        }
//        else {
//            noLoop();
//        }
//    }
//
//
//    public PCanvas setFrameRate(float frameRate) {
//        this.FrameRate = frameRate;
//        frameRate(FrameRate);
//        return this;
//    }
//
//    public float getFrameRate() {
//        return frameRate;
//    }
//
//    public PCanvas setZoom(float x, float y, float z) {
//        setPanX(x);
//        setPanY(y);
//        setZoom(z);
//        return this;
//    }
//
//    public PCanvas setZoom(float f) {
//        zoom = f;
//        return this;
//    }
//
//    public float getZoom() {
//        return zoom;
//    }
//
//    /** zoom to a rectangular region */
//    public void setZoom(float cx, float cy, float width, float height) {
//        //TODO add margin, right-click zoom out
//
//        //System.out.println("auto-zoom: " + cx + " " + cy + " " + width + " " + height);
//
//        //https://github.com/automenta/automenta.spacegraphj/blob/master/spacegraph/automenta/spacegraph/control/FractalControl.java#L128
//
//       // float targetZ = getTargetDepth(width, height); //TODO calculate correct height
//        float targetZ = this.width / Math.max(width,height);
//
//        //System.out.print(getPanX() + " " + getPanY() + " " + getZoom() + " --> ");
//        float tx = (cx) * targetZ;
//        float ty = (cy) * targetZ;
//        //System.out.println(tx + " " + ty + " " + targetZ);
//        setZoom(tx, ty, targetZ);
//
//    }
//    /*static float getTargetDepth(float width, float height) {
//        float zoomDilation = 1.0f;
//        float r = Math.max(width, height) / 2.0f * zoomDilation;
//        final float focus = (float)Math.PI/4f;
//        return (float) (r * Math.sin(Math.PI / 2.0 - focus / 2.0) / Math.sin(focus / 2.0));
//    }*/
//
//    class Hnav {
//
//        private boolean md = false;
//
//        void mousePressed() {
//            md = true;
//            if (mouseButton == RIGHT) {
//                savepx = mouseX;
//                savepy = mouseY;
//                if (!renderEveryFrame)
//                    SwingUtilities.invokeLater(PCanvas.this::predraw);
//            }
//        }
//
//        void mouseReleased() {
//
//            md = false;
//        }
//
//        void mouseDragged() {
//            if (mouseButton == RIGHT) {
//                difx += (mouseX - savepx);
//                dify += (mouseY - savepy);
//                savepx = mouseX;
//                savepy = mouseY;
//                if (!renderEveryFrame)
//                    SwingUtilities.invokeLater(PCanvas.this::predraw);
//            }
//        }
//
//        void keyPressed() {
//            if ((keyToo && key == 'w') || keyCode == UP) {
//                dify += (camspeed);
//            }
//            if ((keyToo && key == 's') || keyCode == DOWN) {
//                dify += (-camspeed);
//            }
//            if ((keyToo && key == 'a') || keyCode == LEFT) {
//                difx += (camspeed);
//            }
//            if ((keyToo && key == 'd') || keyCode == RIGHT) {
//                difx += (-camspeed);
//            }
//            if (!EnableZooming) {
//                return;
//            }
//            if (key == '-' || key == '#') {
//                float zoomBefore = zoom;
//                zoom *= scrollcammult;
//                difx = (difx) * (zoom / zoomBefore);
//                dify = (dify) * (zoom / zoomBefore);
//            }
//            if (key == '+') {
//                float zoomBefore = zoom;
//                zoom /= scrollcammult;
//                difx = (difx) * (zoom / zoomBefore);
//                dify = (dify) * (zoom / zoomBefore);
//            }
//            if (!renderEveryFrame)
//                SwingUtilities.invokeLater(PCanvas.this::predraw);
//            predrawing = false;
//        }
//
//        void Init() {
//            difx = -width / 2;
//            dify = -height / 2;
//        }
//
//        void mouseScrolled() {
//            if (!EnableZooming) {
//                return;
//            }
//            float zoomBefore = zoom;
//            if (mouseScroll > 0) {
//                zoom *= scrollcamspeed;
//            } else {
//                zoom /= scrollcamspeed;
//            }
//            difx = (difx) * (zoom / zoomBefore);
//            dify = (dify) * (zoom / zoomBefore);
//            if (!renderEveryFrame)
//                SwingUtilities.invokeLater(PCanvas.this::predraw);
//            predrawing = false;
//        }
//
//        void applyTransform(PGraphics g) {
//            g.translate(difx + 0.5f * width, dify + 0.5f * height);
//            g.scale(zoom, zoom);
//        }
//    }
//
////    ////Object management - dragging etc.
////    class Hsim {
////
////        ArrayList obj = new ArrayList();
////
////        void Init() {
////            smooth();
////        }
////
////        void mousePressed() {
////            if (mouseButton == LEFT) {
////                checkSelect();
////            }
////        }
////        boolean dragged = false;
////
////        void mouseDragged() {
////            if (mouseButton == LEFT) {
////                dragged = true;
////                dragElems();
////            }
////        }
////
////        void mouseReleased() {
////            dragged = false;
////            //selected = null;
////        }
////
////        void dragElems() {
////            /*
////             if (dragged && selected != null) {
////             selected.x = hnav.MouseToWorldCoordX(mouseX);
////             selected.y = hnav.MouseToWorldCoordY(mouseY);
////             hsim_ElemDragged(selected);
////             }
////             */
////        }
////
////        void checkSelect() {
////            /*
////             double selection_distanceSq = selection_distance*selection_distance;
////             if (selected == null) {
////             for (int i = 0; i < obj.size(); i++) {
////             Vertex oi = (Vertex) obj.get(i);
////             float dx = oi.x - hnav.MouseToWorldCoordX(mouseX);
////             float dy = oi.y - hnav.MouseToWorldCoordY(mouseY);
////             float distanceSq = (dx * dx + dy * dy);
////             if (distanceSq < (selection_distanceSq)) {
////             selected = oi;
////             hsim_ElemClicked(oi);
////             return;
////             }
////             }
////             }
////             */
////        }
////    }
//
//}
