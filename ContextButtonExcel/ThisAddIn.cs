using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Excel = Microsoft.Office.Interop.Excel;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Excel;
using Microsoft.Office.Core;
using System.IO.Pipes;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace ContextButtonExcel
{
    public partial class ThisAddIn
    {
        public CommandBarButton myButton;
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            Application.SheetBeforeRightClick += Application_SheetBeforeRightClick;
        }
        private void Application_SheetBeforeRightClick(object Sh, Excel.Range Target, ref bool Cancel)
        {
            var contextMenu = Application.CommandBars["Cell"];
            foreach (CommandBarControl control in contextMenu.Controls)
            {
                if (control is CommandBarButton && ((CommandBarButton)control).Tag == "MyButtonTag")
                {
                    control.Delete();
                }
            }
            myButton = (CommandBarButton)contextMenu.Controls.Add();
            myButton.Caption = "Call Number";
            myButton.Tag = "MyButtonTag";
            myButton.Click += MyButton_Click;
        }
        private void MyButton_Click(CommandBarButton Ctrl, ref bool CancelDefault)
        {
            try
            {
                Excel.Application excelApp = this.Application;
                Excel.Range selectedRange = excelApp.Selection;
                var value = Convert.ToString(selectedRange.Text);
                if (!String.IsNullOrEmpty(value))
                {
                    Thread.Sleep(1000);
                    SendInSystray(value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private async Task SendInSystray(string phone)
        {
            try
            {
                using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "VoiceXPipe", PipeDirection.Out))
                {
                    if (!pipeClient.IsConnected)
                    {
                        await pipeClient.ConnectAsync();
                    }
                    using (StreamWriter sw = new StreamWriter(pipeClient))
                    {
                        await sw.WriteLineAsync(phone);
                        await sw.FlushAsync();
                        sw.Close();
                        sw.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void ThisAddIn_Shutdown(object sender, EventArgs e)
        {

        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
