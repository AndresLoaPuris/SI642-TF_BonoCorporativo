using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SI642_BonoCorporativo.Models
{
	public class UserLogin
	{
		public int Id { get; set; }
		[Required(ErrorMessage = "Error: Debe ingresar el campo DNI")]
		[StringLength(8, MinimumLength = 8, ErrorMessage = "Error: Solo 8 Digitos")]
		[RegularExpression(@"[0-9]{1,9}(\.[0-9]{0,2})?$", ErrorMessage = "Error: Solo Números")]
		[Display(Name = "DNI")]
		public string DNI { get; set; }
		[Required(ErrorMessage = "Error: Debe ingresar el campo Contraseña")]
		[Display(Name = "Contraseña")]
		public string Password { get; set; }
	}
}