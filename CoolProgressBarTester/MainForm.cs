using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace CoolProgressBarTester
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            ResetState();
        }

        private void ResetState()
        {
            btnStart.Enabled = true;
            btnStop.Enabled = false;
        }

        private void StartState()
        {
            coolProgressBarCtrl1.Value = 0;
            btnStart.Enabled = false;
            btnStop.Enabled = true;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            StartState();
            backgroundWorker1.RunWorkerAsync();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }

        private void OnDoWork(object sender, DoWorkEventArgs e)
        {
            for ( int i = 1; i <= 100; i++ )
            {
                if ( backgroundWorker1.CancellationPending )
                    break;

                Thread.Sleep(100);
                backgroundWorker1.ReportProgress(i);
            }
        }

        private void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            coolProgressBarCtrl1.Value = e.ProgressPercentage;
        }

        private void OnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ResetState();
        }

    }
}
