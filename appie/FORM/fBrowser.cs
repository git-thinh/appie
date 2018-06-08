using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace appie
{
    public class fBrowser: Form
    {
        TextBox m_url_textBox;
        TabControl m_tab;
        WebBrowser m_browser;
        public fBrowser() {
            m_url_textBox = new TextBox() {
                Dock = DockStyle.Top,
            };

            m_browser = new WebBrowser() {
                Dock = DockStyle.Fill,
                ScriptErrorsSuppressed = true,
            };
            m_tab = new TabControl() {
                Dock = DockStyle.Fill,
            };
            var m_tab_Main = new TabPage()
            {                
            };
            m_tab_Main.Controls.Add(m_browser);
            m_tab.Controls.AddRange(new Control[] {
                m_tab_Main,
            });
            this.Controls.AddRange(new Control[] {
                m_tab,
                m_url_textBox,
            });
            this.Shown += f_form_Shown;
        }

        private void f_form_Shown(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            m_browser.Navigate("file:///G:/_EL/Document/data_el2/book/Cau_truc_tieng_anh_The_Windy.pdf");
        }
    }
}
