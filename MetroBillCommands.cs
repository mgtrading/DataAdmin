using DevComponents.DotNetBar;

namespace DataAdmin
{
    /// <summary>
    /// Represents all application commands.
    /// </summary>
    public class MetroBillCommands
    {
        private StartControlCommands _startControlCommands = new StartControlCommands();
        private AddUserControlCommands _addUserControlCommands = new AddUserControlCommands();
        private EditUserControlCommands _editUserControlCommands = new EditUserControlCommands();
        

        public StartControlCommands StartControlCommands
        {
            get { return _startControlCommands; }
            set { _startControlCommands = value; }
        }

        public AddUserControlCommands AddUserControlCommands
        {
            get { return _addUserControlCommands; }
            set { _addUserControlCommands = value; }
        }

        public EditUserControlCommands EditUserControlCommands
        {
            get { return _editUserControlCommands; }
            set { _editUserControlCommands = value; }
        }
    }

    public class StartControlCommands
    {
        public Command Logon { get; set; }
        public Command Exit { get; set; }
    }

    public class AddUserControlCommands
    {
        public Command Add { get; set; }
        public Command Cancel { get; set; }
    }

    public class EditUserControlCommands
    {
        public Command SaveChanges { get; set; }
        public Command Cancel { get; set; }
    }
}
