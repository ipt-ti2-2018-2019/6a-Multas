using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Multas.Models {
   public class Agentes {

      /// atributos:
      /// - nº agente
      /// - nome
      /// - esquadra
      /// - foto

      public Agentes() {
         // criar o objeto 'ListaDeMultas'
         ListaDeMultas = new HashSet<Multas>();
      }

      [Key]
      public int ID { get; set; }

      [Required(ErrorMessage = "o Nome é de preenchimento obrigatório.")]
      [StringLength(40)]
      [RegularExpression("[A-ZÁÉÍÓÚÂ][a-záéíóúàèìòùäëïöüãõâêîôûçñ]+(( | e | de | da | das | do | dos |-|'|d')[A-ZÁÉÍÓÚÂ][a-záéíóúàèìòùäëïöüãõâêîôûçñ]*){1,3}",
                         ErrorMessage = "só são aceites palavras, começadas por Maiúscula, " +
                                       "separadas por um espeço em branco.")]
      public string Nome { get; set; }

      [Required(ErrorMessage = "a Esquadra é de preenchimento obrigatório.")]
      [StringLength(30)]
      //   [RegularExpression("Torres Novas|Tomar|Entroncamento",
      //                      ErrorMessage = " só é aceite Torres Novas ou Tomar ou Entroncamento")]
      public string Esquadra { get; set; }

      [StringLength(40)]
      public string Fotografia { get; set; }

      // lista das multas associadas ao Agente
      public virtual ICollection<Multas> ListaDeMultas { get; set; }
      // a palavra 'virtual' ativa o chamado Lazy Loading

      //*************************************************************
      // definir uma "chave forasteira" para a tabela da autenticação
      //   [Required]
      public string UserName { get; set; }

   }
}





