//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RPM
{
    using System;
    using System.Collections.Generic;
    
    public partial class Shipments
    {
        public int IDShipment { get; set; }
        public System.DateTime DateShipment { get; set; }
        public int Amount { get; set; }
        public int IDParty { get; set; }
        public int IDStorage { get; set; }
    
        public virtual Party Party { get; set; }
        public virtual Storage Storage { get; set; }
    }
}
