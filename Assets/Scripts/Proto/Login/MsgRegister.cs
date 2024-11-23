using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MsgRegister : MsgBase
{
    public MsgRegister()
    {
        protoName = "MsgRegister";
    }
    // 请求
    public string id = "";  // game_id
    public string pw = "";  // 密码
    public string nickname = ""; // 新增:昵称
    public DateTime registration_date; // 新增:注册时间

    // 响应
    public bool result = false;
    public string error_msg = ""; // 新增:错误信息
}

