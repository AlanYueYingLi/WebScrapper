# Specifications

1. Main
1-1-1. Version controlled by Git and upload to GitHub or GitLab.
1-1-2. Apply SourceTree to provide UI.
1-2-1.
1-3-1. Parallel loops implementation.
2. Logger
2-1-1. void WriteLog(DateTime, Object, Message)
2-1-2. LogMsg: structure shall includes DateTime, Object, Message.
2-1-3. LogMsg format: DateTime	Object	Message (Separation char is '\t'). Each LogMsg separated by NewLine.
2-1-4. DateTime format: "HH:mm:ss.fff".
2-2-1. Instance name is required.
2-2-2. File format: .txt.
2-2-3. File naming rule: "yyyyMMdd". Same date shall share same log file.
2-2-4. Folder path rule: application directory + "Log" + instance creation name. Same module shall share same folder.
2-3-1. Timer: Trigger interval = 1000ms.
3. IniHandler
3-1-1. Instance name is required.
3-1-2. File format: .ini.
3-1-3. File naming rule: instance name.
3-1-4. Folder path rule: application directory + "ini" + instance creation name. Each module shall have its unique name.
3-1-5. Dictionary<string, string> ReadSetting().
3-2-1. void WriteSetting(Dictionary<string, string>).
3-2-2. The payload format might be replaced by JSON format.
4. Scrapter
5. DBHandler
6. Parser
