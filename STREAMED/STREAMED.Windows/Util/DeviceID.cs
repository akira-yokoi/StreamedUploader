using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.System.Profile;

namespace STREAMED.Util
{
  class DeviceID
  {
    #region public static void CreateDeviceID() : デバイスIDの作成
    /// <summary>
    /// デバイスIDの作成
    /// </summary>
    public static void CreateDeviceID()
    {
      var key = SettingUtil.getString(SettingUtil.UNIQUE_ID_KEY);
      if(string.IsNullOrEmpty(key))
      {
        key = makeDeviceID();
        SettingUtil.putString(SettingUtil.UNIQUE_ID_KEY, key);
      }
    }
    #endregion

    #region private static string DeviceID() : DeviceID取得
    /// <summary>
    /// ４５ケタのDeviceID取得
    /// </summary>
    /// <returns></returns>
    private static string makeDeviceID()
    {
      var token = HardwareIdentification.GetPackageSpecificToken(null);
      var hardid = token.Id;

      var rdr = DataReader.FromBuffer(hardid);
      byte[] arr = new byte[hardid.Length];
      rdr.ReadBytes(arr);

      var sb = new StringBuilder();
      foreach (var bb in arr)
      {
        sb.Append(bb.ToString());
      }

      string id = sb.ToString();

      var algo = HashAlgorithmProvider.OpenAlgorithm("SHA1");
      IBuffer buf = CryptographicBuffer.ConvertStringToBinary(id, BinaryStringEncoding.Utf8);
      var hash = algo.HashData(buf);
      string hashs = CryptographicBuffer.EncodeToHexString(hash);

      return hashs;
    }
    #endregion
  }
}
