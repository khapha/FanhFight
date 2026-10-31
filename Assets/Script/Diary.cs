using UnityEngine;

// NHẬT KÝ BƯỚM (dữ liệu): mỗi con gián được "cứu" (đập/xịt) hóa thành một chú bướm
// có tên riêng lưu trong PlayerPrefs — biến việc đập gián thành cứu bạn bướm.
// Đủ 8 chú thì mở tiệc hóa hoa ở menu chính. Vui vẻ, không đáng sợ.
public static class Diary
{
    // tên bướm cute, gọi lần lượt theo số lượng đã cứu
    public static readonly string[] Names = {
        "Bơ", "Mochi", "SuSu", "Bông", "Kem", "Dừa", "Chanh", "Mận",
        "Mochi II", "Nấm", "Khoai", "Bắp", "Cà rốt", "Đậu", "Mè", "Bảy sắc"
    };

    const string CountKey = "diaryRescued";

    public static int Rescued => PlayerPrefs.GetInt(CountKey, 0);

    // một chú bướm mới gia nhập; trả về tên của chú ấy
    public static string Rescue(string who = null)
    {
        int count = PlayerPrefs.GetInt(CountKey, 0);
        string name = Names[count % Names.Length];
        if (who != null) name += " (" + who + ")";
        PlayerPrefs.SetInt(CountKey, count + 1);
        PlayerPrefs.Save();
        Debug.Log("DIARY: rescued " + name + " (#" + (count + 1) + ")");
        return name;
    }

    // tên hiển thị của chú bướm thứ i (chưa cứu thì ẩn)
    public static string DisplayName(int i) => i < Rescued
        ? (i < Names.Length ? Names[i] : "Butterfly #" + (i + 1))
        : "???";

    // đủ 8 chú là mở tiệc hóa hoa
    public static bool PartyUnlocked => Rescued >= 8;

    // xóa nhật ký (dùng bởi AutoPlay/kiểm thử)
    public static void Reset() => PlayerPrefs.DeleteKey(CountKey);
}
