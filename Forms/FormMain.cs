using System;
using System.Drawing;
using System.Windows.Forms;
using DataAdmin.DbDataManager;
using DataAdmin.Properties;
using DataAdmin.ServerManager;
using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Metro;

namespace DataAdmin.Forms
{
    public partial class FormMain : MetroAppForm
    {
        private readonly StartControl _startControl;
        private AddUserControl _addUserControl;
        private readonly MetroBillCommands _commands;

        #region Basic function (Constructor, Show, Closing, Resize, Notify)

        public FormMain()
        {
            InitializeComponent();
            metroShellMain.SelectedTab = metroTabItem_home;
            ToastNotification.ToastBackColor = Color.SteelBlue;
            ToastNotification.DefaultToastPosition = eToastPosition.BottomCenter;

            SuspendLayout();

            _commands = new MetroBillCommands
            {
                StartControlCommands = { Logon = new Command(), Exit = new Command() },
                AddUserControlCommands = { Add = new Command(), Cancel = new Command() }
            };            
            //**
            _commands.StartControlCommands.Logon.Executed += StartControl_LogonClick;
            _commands.StartControlCommands.Exit.Executed += StartControl_ExitClick;

            _commands.AddUserControlCommands.Add.Executed += AddNewUserControl_AddClick;
            _commands.AddUserControlCommands.Cancel.Executed += AddNewUserControl_CancelClick;
            //**
            _startControl = new StartControl {Commands = _commands};
            //_addUserControl = new AddUserControl {Commands = _commands, Tag = 0};

            Controls.Add(_startControl);
            _startControl.BringToFront();            
            _startControl.SlideSide = DevComponents.DotNetBar.Controls.eSlideSide.Right;

            
            ResumeLayout(false);
        }

        private void FormMain_Shown(object sender, EventArgs e)
        {
            var color = Color.SteelBlue;
            ui_home_labelX1.ForeColor = color;
            ui_home_labelX2.ForeColor = color;
            ui_home_labelX3.ForeColor = color;
            ui_home_labelX4.ForeColor = color;

            ui_user_labelX1.ForeColor = color;
            ui_user_labelX2.ForeColor = color;
            ui_symbols_labelX1.ForeColor = color;
            ui_symbols_labelX2.ForeColor = color;
            ui_logs_labelX_logs.ForeColor = color;

            notifyIcon1.Icon = Icon;
            if (_startControl!=null)
                _startControl.ui_textBoxX_login.Focus();
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Hide();
            e.Cancel = false;
            Settings.Default.Save();
            /*
            if (MessageBox.Show(@"Do you really want to exit program?", @"DataAdmin", MessageBoxButtons.YesNo,
                                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                Hide();
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
                //
            }*/
        }

        private void metroShell1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                ShowInTaskbar = Settings.Default.ShowInTaskBar;
            }
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;
                ShowInTaskbar = true;
            }
        }

        #endregion
        
        #region UI Code

        private void StartControl_ExitClick(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void StartControl_LogonClick(object sender, EventArgs e)
        {

            Settings.Default.connectionUser = _startControl.ui_textBoxX_login.Text;
            Settings.Default.connectionPassword = _startControl.ui_textBoxX_password.Text;
            Settings.Default.connectionHost = _startControl.ui_textBoxX_host.Text;
            Settings.Default.connectionDB = _startControl.ui_textBoxX_db.Text;

            if (DataManager.Initialize(Settings.Default.connectionHost, Settings.Default.connectionDB, Settings.Default.connectionUser, Settings.Default.connectionPassword))
            {
                _startControl.IsOpen = false;
                SocketServer.SetupServer();
            }
            else
            {
                ToastNotification.Show(_startControl, @"Wrong login or password");
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            UpdateControlsSizeAndLocation();
            base.OnLoad(e);
        }

        private Rectangle GetStartControlBounds()
        {
            var captionHeight = metroShellMain.MetroTabStrip.GetCaptionHeight() + 2;
            var borderThickness = GetBorderThickness();
            return new Rectangle((int)borderThickness.Left, captionHeight, Width - (int)borderThickness.Horizontal, Height - captionHeight - 1);
        }

        private void UpdateControlsSizeAndLocation()
        {
            if (_startControl != null)
            {
                if (!_startControl.IsOpen)
                    _startControl.OpenBounds = GetStartControlBounds();
                else
                    _startControl.Bounds = GetStartControlBounds();
                if (!IsModalPanelDisplayed)
                    _startControl.BringToFront();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            UpdateControlsSizeAndLocation();
            base.OnResize(e);
        }

        private void metroShell1_LogOutButtonClick(object sender, EventArgs e)
        {
            if (AnyControlsIsOpen()) return;

            _startControl.IsOpen = true;
            SocketServer.CloseAllSockets();            
        }
        
        private bool AnyControlsIsOpen()
        {
            return _addUserControl!=null && _addUserControl.IsOpen;
        }
        
        private void metroShell1_SettingsButtonClick(object sender, EventArgs e)
        {
            var frm = new FormSettings();
            frm.ShowDialog();
        }

        #endregion

        #region MAIN

        #endregion

        #region SYMBOLS

        #endregion

        #region USERS

        private void ui_users_buttonX_add_Click(object sender, EventArgs e)
        {
            ui_users_buttonX_add.Enabled = false;

            _addUserControl = new AddUserControl { Commands = _commands, Tag = 0 };
            ShowModalPanel(_addUserControl, DevComponents.DotNetBar.Controls.eSlideSide.Right);                            
        }

        private void AddNewUserControl_AddClick(object sender, EventArgs e)
        {

            ui_users_buttonX_add.Enabled = true;
        }

        private void AddNewUserControl_CancelClick(object sender, EventArgs e)
        {
            CloseModalPanel(_addUserControl, DevComponents.DotNetBar.Controls.eSlideSide.Right);
            _addUserControl.Dispose();
            _addUserControl = null;
            ui_users_buttonX_add.Enabled = true;            
        }

        #endregion

        #region LOGS

        #endregion


    }
}