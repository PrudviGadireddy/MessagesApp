using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MessagesApp.Models
{
    public class myMessage
    {
        [Key]
        public int ID { get; set; }
        public string msg { get; set; } = null!;
        public string JSONmsg { get; set; } = "";

        public string JSONmsgTrimmed
        {
            get
            {
                if (this.JSONmsg.Length > 20)
                    return this.JSONmsg.Substring(0, 20) + "...";
                else
                    return this.JSONmsg;
            }
        }
        public string msgTrimmed
        {
            get
            {
                if (this.msg.Length > 20)
                    return this.msg.Substring(0, 20) + "...";
                else
                    return this.msg;
            }
        }

    }
}
