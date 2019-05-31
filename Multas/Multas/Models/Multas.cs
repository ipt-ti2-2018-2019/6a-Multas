﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Multas.Models {
   public class Multas {

      public int ID { get; set; }

      public string Infracao { get; set; }

      public string LocalDaMulta { get; set; }

      public decimal ValorMulta { get; set; }

      public DateTime DataDaMulta { get; set; }

      // ******************************************************************
      // criação das chaves forasteiras (FKs)

      // FK para a tabela dos Agentes
      [ForeignKey("Agente")]
      public int AgenteFK { get; set; }
      public virtual Agentes Agente { get; set; }

      // FK para a tabela dos Condutores
      [ForeignKey("Condutor")]
      public int CondutorFK { get; set; }
      public virtual Condutores Condutor { get; set; }

      // FK para a tabela das Viaturas
      [ForeignKey("Viatura")]
      public int ViaturaFK { get; set; }
      public virtual Viaturas Viatura { get; set; }


   }
}