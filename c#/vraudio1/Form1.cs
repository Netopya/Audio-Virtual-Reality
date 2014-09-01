using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vannatech.CoreAudio.Constants;
using Vannatech.CoreAudio.Interfaces;
using Vannatech.CoreAudio.Enumerations;
using System.Runtime.InteropServices;
using System.Net;

namespace vraudio1
{
    public partial class Form1 : Form
    {
        const uint CLSCTX_ALL = 0x1 | 0x2 | 0x4 | 0x10;
        IAudioEndpointVolume manager;

        WebClient client = new WebClient();

        TreeNode headingNode;
        TreeNode currentVolNode;
        TreeNode leftVol;
        TreeNode rightVol;

        public Form1()
        {

            //THANK YOU: http://www.codeproject.com/Questions/332216/Problems-using-Windows-Core-Audio

            //Get device enumerator ID
            var type =
            Type.GetTypeFromCLSID(
                Guid.Parse(ComCLSIDs.MMDeviceEnumeratorCLSID));

            //initialize device enumerator
            IMMDeviceEnumerator deviceEnumerator = 
            (IMMDeviceEnumerator)Activator.CreateInstance( type );

            //get the main audio device from the enumerator
            IMMDevice device;
            Marshal.ThrowExceptionForHR(
                deviceEnumerator.GetDefaultAudioEndpoint(
                    EDataFlow.eRender,
                    ERole.eMultimedia,
                    out device));

            //initialize the audio manager from the main device
            object obj;
            Marshal.ThrowExceptionForHR(
                device.Activate(
                    Guid.Parse(ComIIDs.IAudioEndpointVolumeIID),
                    CLSCTX_ALL,
                    IntPtr.Zero,
                    out obj));
            manager = (IAudioEndpointVolume)obj;

            //release the enumerator and main device (we don't need them anymore)
            Marshal.ThrowExceptionForHR(Marshal.FinalReleaseComObject(deviceEnumerator));
            Marshal.ThrowExceptionForHR(Marshal.FinalReleaseComObject(device));
            
            InitializeComponent();

            //setup the tree view control
            headingNode = new TreeNode("Heading: LOADING!");
            currentVolNode = new TreeNode("Current Volume: ");
            leftVol = new TreeNode("Left Volume: ");
            rightVol = new TreeNode("Right Volume: ");

            currentVolNode.Nodes.Add(leftVol);
            currentVolNode.Nodes.Add(rightVol);

            treeView1.Nodes.Add(headingNode);
            treeView1.Nodes.Add(currentVolNode);

            treeView1.ExpandAll();

            //begin the background worker
            backgroundWorker1.RunWorkerAsync();
        }


        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                //read and report the information from the text file
                //MODIFY THIS URL TO MATCH THE URL OF YOUR TEXT FILE
                backgroundWorker1.ReportProgress(0, client.DownloadString("http://192.168.1.150/vraudio/ori.txt"));
                System.Threading.Thread.Sleep(100);
            }
            
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            float heading;
            try
            {
                //retrieve the orientation from the progress report
                heading = int.Parse((string)e.UserState);
            }
            catch (Exception ex)
            {
                heading = 0;
            }

            try
            {
                headingNode.Text = "Heading: " + heading;
                trackBar1.Value = (int)(360 - (heading + 180) % 360); //set the trackbar position
            }
            catch (System.ObjectDisposedException ex)
            {
                return;
            }

            //get the current volume
            float currentVol;
            manager.GetMasterVolumeLevelScalar(out currentVol);

            currentVolNode.Text = "Current Volume: " + currentVol;

            float gain = 20;
            float left = currentVol;
            float right = currentVol;
            float adjustedVol = currentVol - (currentVol / gain);

            //calculate the respective audio channel volume based on the heading
            if (heading < 90)
            {
                adjustedVol = adjustedVol - adjustedVol * (heading / 90);
                right = adjustedVol;
                left = currentVol;
            }
            else if (heading > 270)
            {

                adjustedVol = adjustedVol - adjustedVol * ((360 - heading) / 90);
                left = adjustedVol;
                right = currentVol;


            }

            leftVol.Text = "Left Volume: " + left;
            rightVol.Text = "Right Volume: " + right;

            //set the volumes of the left and right audio channels
            manager.SetChannelVolumeLevelScalar(0, right, Guid.Parse(ComIIDs.IAudioEndpointVolumeIID));
            manager.SetChannelVolumeLevelScalar(1, left, Guid.Parse(ComIIDs.IAudioEndpointVolumeIID));
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //stop the background work
            backgroundWorker1.CancelAsync();

            //get the current volumes
            float currentVol;
            manager.GetMasterVolumeLevelScalar(out currentVol);

            //reset the audio balance
            manager.SetChannelVolumeLevelScalar(0, currentVol, Guid.Parse(ComIIDs.IAudioEndpointVolumeIID));
            manager.SetChannelVolumeLevelScalar(1, currentVol, Guid.Parse(ComIIDs.IAudioEndpointVolumeIID));

            //release the audio manager
            Marshal.ThrowExceptionForHR(
                Marshal.FinalReleaseComObject(manager));
        }

        
    }
}
