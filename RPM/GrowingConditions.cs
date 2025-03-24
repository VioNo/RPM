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
    
    public partial class GrowingConditions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public GrowingConditions()
        {
            this.Fermentation = new HashSet<Fermentation>();
        }
    
        public int IDGrowingConditions { get; set; }
        public int IDGrapeVarieties { get; set; }
        public int IDSoil { get; set; }
        public int IDWater { get; set; }
        public int IDClimate { get; set; }
        public string Description { get; set; }
    
        public virtual Climate Climate { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Fermentation> Fermentation { get; set; }
        public virtual GrapeVarieties GrapeVarieties { get; set; }
        public virtual Water Water { get; set; }
        public virtual Soil Soil { get; set; }
    }
}
