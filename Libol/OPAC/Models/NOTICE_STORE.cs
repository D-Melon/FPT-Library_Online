//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OPAC.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class NOTICE_STORE
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public System.DateTime CreateTime { get; set; }
        public int TypeID { get; set; }
        public int LibID { get; set; }
    }
}