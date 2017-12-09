using System.Collections.Generic;

namespace fds
{
  internal class FDSFile
  {
    private readonly Dictionary<int, string> FileTypes = new Dictionary<int, string>()
    {
      {
        0,
        "Program"
      },
      {
        1,
        "Character"
      },
      {
        2,
        "Name table"
      }
    };
    public int FileNumber;
    public int FileID;
    public string FileName;
    public int FileLoadAddress;
    public int FileSize;
    public int FileType;
    public long FileDataStart;

    public string[] GetInfo()
    {
      string[] strArray = new string[6]
      {
        this.FileNumber.ToString(),
        this.FileID.ToString(),
        this.FileName,
        this.FileLoadAddress.ToString("x4"),
        this.FileSize.ToString(),
        null
      };
      if (!this.FileTypes.TryGetValue(this.FileType, out strArray[5]))
        strArray[5] = "Unknown";
      return strArray;
    }
  }
}
