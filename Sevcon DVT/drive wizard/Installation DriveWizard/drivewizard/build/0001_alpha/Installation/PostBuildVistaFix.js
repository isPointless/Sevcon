    // PostBuildVistaFix.js <msi-file>
    // Performs a post-build fixup of an msi to change all deferred custom actions to NoImpersonate and adds an entry in the error table
    // Constant values from Windows Installer

    var msiOpenDatabaseModeTransact = 1;
    var msiViewModifyInsert = 1
    var msiViewModifyUpdate = 2
    var msiViewModifyAssign = 3
    var msiViewModifyReplace = 4
    var msiViewModifyDelete = 6

    var msidbCustomActionTypeInScript = 0x00000400;
    var msidbCustomActionTypeNoImpersonate = 0x00000800;

    if (WScript.Arguments.Length != 1)
    {
        WScript.StdErr.WriteLine(WScript.ScriptName + " filename.msi");
        WScript.Quit(1);
    }

    var filespec = WScript.Arguments(0);
    var installer = WScript.CreateObject("WindowsInstaller.Installer");
    var database = installer.OpenDatabase(filespec, msiOpenDatabaseModeTransact);

    var sql
    var view
    var record
    var errorRecordFound = false;

    try
    {
    // first set the NO_IMPERSONATE bit for all the custom actionss
    sql = "SELECT `Action`, `Type`, `Source`, `Target` FROM `CustomAction`";
    view = database.OpenView(sql);
    view.Execute();
    record = view.Fetch();

    while (record)
    {
        if (record.IntegerData(2) & msidbCustomActionTypeInScript)
        {
            record.IntegerData(2) = record.IntegerData(2) | msidbCustomActionTypeNoImpersonate;
            view.Modify(msiViewModifyReplace, record);
        }
        record = view.Fetch();
    }
    // close the view.
    view.Close();

    //check whether a row exists in the error table, if it doesn't create it

    sql = "SELECT `Error`, `Message` from `Error` where `Error`=1001" ;
    view = database.OpenView(sql);
    view.Execute();
    record = view.Fetch();

    if(record)
    {
        errorRecordFound = true;
    }
    view.Close();

    if( !errorRecordFound )
    {
        // create a new view to insert an error row in the error table so 
        // that an error message is displayed in vista, instead of package error!

        sql = "INSERT INTO `Error` (`Error`, `Message`) VALUES (1001, '[2]')";
        view = database.OpenView(sql);
        view.Execute();
        view.Close();
    }

    database.Commit();
    }
    catch(e)
    {
        WScript.StdErr.WriteLine(e);
        WScript.Quit(1);
    }