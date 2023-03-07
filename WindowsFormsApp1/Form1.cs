using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CP_MockTest_DLL;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {

        FileServer fs = FileServer.GetInstance();


        object obj = new object();
        SemaphoreSlim sl = new SemaphoreSlim(4, 4);
        ReaderWriterLock rw = new ReaderWriterLock();
        CountdownEvent cde = new CountdownEvent(10);
        string path = "txt.txt";



        public Form1()
        {
            InitializeComponent();


        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            init();

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        void init() {

            btnStart.Enabled = false;

            for (int i = 1; i <= 10; i++)
            {
                Thread t = new Thread(GetAndSave);
                t.Start(i);
            }

            new Thread(
                () => {

                    cde.Wait();

                    invoke(() =>
                    {
                        btnStart.Enabled = true;
                        readText();
                    });

                    cde.Reset();
                }
            ).Start();

            
        }

        void GetAndSave(object threadId) {

            sl.Wait();

            int id = (int) threadId;

            // thread start info

            invoke(() =>
            {
                listBox1.Items.Add(string.Format($"Thread {id} starting"));
            });

            // get msg

            byte[] array = fs.GetFile(id);

            rw.AcquireWriterLock(Timeout.Infinite);
            AppendAllBytes(path, array);

            //File.WriteAllBytes(@"D:\WIUT Classes\lvl6\ConcurrentProg\Mocks\bis2_3\txt\nex.txt", array);

            rw.ReleaseWriterLock();



            string msg = Encoding.Default.GetString(array);

            // insert msg

            invoke(() =>
            {
                listBox2.Items.Add(msg);
            });

            // thread finish info

            invoke(() =>
            {
                listBox1.Items.Add(string.Format($"Thread {id} finished"));
            });

            cde.Signal();

            sl.Release();
            


        }
        public static void AppendAllBytes(string path, byte[] bytes)
        {


            using (var stream = new FileStream(path, FileMode.Append))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        void invoke(Action func)
        {
            Invoke(
                new MethodInvoker(
                    () => {
                        func();
                    }
                )
            );
        }


        void readText()
        {
            rw.AcquireReaderLock(Timeout.Infinite);
            string txt = File.ReadAllText(path);

            rw.ReleaseReaderLock();

            textBox1.Text = txt;
        }
    }
}
