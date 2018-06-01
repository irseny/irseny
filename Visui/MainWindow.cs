using System;
using Gtk;


public partial class MainWindow : Gtk.Window
{
    Emgu.CV.VideoCapture capture;


    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {   Build();
        using (var stream = new System.IO.FileStream("gui.glade", System.IO.FileMode.Open))
        {
            Glade.XML gl = Glade.XML.FromStream(stream, "window1", null);
            var box = gl.GetWidget("label1");
            rootContainer.Add(box);
        }


    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        Application.Quit();
        a.RetVal = true;
    }

    protected void OnStart(object sender, EventArgs e)
    {
        if (capture == null)
        {

            int deviceId = idInput.ValueAsInt;
            capture = new Emgu.CV.VideoCapture(deviceId);
            capture.ImageGrabbed += ShowImage;
            capture.Start();
        }
        startButton.Sensitive = false;
        stopButton.Sensitive = true;
    }

    protected void OnStop(object sender, EventArgs e)
    {
        if (capture != null)
        {
            capture.Dispose();
            capture = null;


        }
        startButton.Sensitive = true;
        stopButton.Sensitive = false;
    }

    void ShowImage(object sender, EventArgs e)
    {

        var capture = (Emgu.CV.VideoCapture)sender;
        var imageMatrix = new Emgu.CV.Mat();
        capture.Retrieve(imageMatrix);
        var stream = new System.IO.MemoryStream();
        imageMatrix.Bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
        stream.Position = 0;
        videoOut.Pixbuf = new Gdk.Pixbuf(stream);
        imageMatrix.Dispose();
        videoOut.QueueDraw();

    }

}
