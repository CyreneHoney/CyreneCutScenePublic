namespace CyreneCore.Utils;

public enum LanguageTypeEnum
{
    None = 0,
    CHS = 1,
    CHT = 2,
    EN = 3,
    KR = 4,
    JP = 5,
    FR = 6,
    DE = 7,
    ES = 8,
    PT = 9,
    RU = 10,
    TH = 11,
    VI = 12,
    ID = 13
}

public enum ProcessState
{
    None,
    Pending,
    Processing,
    Completed,
    Failed
}

public enum LogLevel
{
    Debug,
    Info,
    Warn,
    Error
}