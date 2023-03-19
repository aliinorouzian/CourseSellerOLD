using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseSeller.DataLayer.Entities.Users
{
    public class Role
    {
        [Key]
        public int RoleID { get; set; }

        [Display(Name = "عنوان نقش")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید.")]
        [MaxLength(200, ErrorMessage = "{0} نمی تواند بیشتر از {1} کاراکتر باشد.")]
        public string RoleTitle { get; set; }


        #region Relations

        public List<UserRole> UserRoles { get; set; }

        #endregion
    }
}
