using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Multas.Models;

namespace Multas.Controllers {

   [Authorize] // só pessoas autenticadas podem aceder ao seu conteúdo
   public class AgentesController : Controller {
      private MultasDB db = new MultasDB();

      // GET: Agentes
      [Authorize(Roles = "RecursosHumanos,Agente")] // só as pessoas que forem
      // do role 'Agente' OU do role 'RecursosHumanos' podem aceder

      //[Authorize(Roles = "Agente")]           // só tem acesso as pessoas que forem 
      //[Authorize(Roles = "RecursosHumanos")]  // de RecursosHumanos E Agente
      public ActionResult Index() {

         // LINQ
         // SELECT * FROM Agentes ORDER BY ID DESC   <--- só as pessoas dos recursos humanos
         var listaDeAgentes = db.Agentes
                                .OrderByDescending(a => a.ID)
                                .ToList();

         // se for apenas agente
         //SELECT * FROM Agente WHERE UserName = username da pessoa autenticada
         if(!User.IsInRole("RecursosHumanos")) {
            // vou restringir a listagem inicial apenas aos dados do Agente
            //  listaDeAgentes = listaDeAgentes.Where(a => a.UserName == User.Identity.Name).ToList();

            // redirecionar para página dos detalhes
            int idAgente = db.Agentes
                           .Where(a => a.UserName == User.Identity.Name)
                           .FirstOrDefault()
                           .ID;
            return RedirectToAction("Details", new { id = idAgente });


         }

         return View(listaDeAgentes);
      }

      // GET: Agentes/Details/5
      public ActionResult Details(int? id) {
         if(id == null) {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         Agentes agente = db.Agentes.Find(id);

         if(agente == null) {
            return HttpNotFound();
         }

         /// posso aceder aos dados dos agentes, quando:
         ///   - for do role 'recursos humanos'
         ///   - for do role ' gestor multas'
         ///   - for o dono dos dados
         if(User.IsInRole("RecursosHumanos") ||
            User.IsInRole("GestorMultas") ||
            agente.UserName == User.Identity.Name) {
            // envia os dados do AGENTE para a View
            return View(agente);
         }
         else{
            // estou a tentar aceder a dados não autorizados
            return RedirectToAction("Index");
         }
      }

      // GET: Agentes/Create
      [Authorize(Roles = "RecursosHumanos")]
      public ActionResult Create() {
         return View();
      }

      // POST: Agentes/Create
      // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
      // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
      [HttpPost]
      [ValidateAntiForgeryToken]
      [Authorize(Roles = "RecursosHumanos")]
      public ActionResult Create([Bind(Include = "Nome,Esquadra")] Agentes agente,
                                 HttpPostedFileBase fotografia) {

         // vars auxiliares
         string caminho = "";
         bool imagemValida = false;

         /// foi fornecido um ficheiro?
         if(fotografia == null) {
            // a foto não existe
            // vou atribuir uma fotografia por defeito
            agente.Fotografia = "nouser.jpg";
         }
         else {
            // existe ficheiro
            /// é uma imagem (fotografia)?
            // aceitamos JPEG e PNG
            if(fotografia.ContentType == "image/jpeg" ||
               fotografia.ContentType == "image/png") {
               // estamos perante uma Foto válida
               /// se é fotografia, 
               ///     guardar a imagem e 
               ///       - definir um nome
               Guid g;
               g = Guid.NewGuid();
               string extensaoDoFicheiro = Path.
                                           GetExtension(fotografia.FileName).
                                           ToLower();
               string nomeFicheiro = g.ToString() + extensaoDoFicheiro;

               ///       - definir um local onde a guardar
               caminho = Path.Combine(Server.MapPath("~/Imagens/"), nomeFicheiro);

               ///     associar ao agente
               agente.Fotografia = nomeFicheiro;

               // marca o ficheiro como válido
               imagemValida = true;
            }
            else {
               /// se não é um ficheiro do tipo imagem (JPEG ou PNG), 
               ///     atribuir ao agente uma 'imagem por defeito'
               agente.Fotografia = "nouser.jpg";
            }
         }

         // avalia se os dados fornecidos estão de acordo com o modelo
         if(ModelState.IsValid) {
            // adicionar os dados do novo Agente ao Modelo
            db.Agentes.Add(agente);
            try {
               // guardar os dados na BD
               db.SaveChanges();
               // guardar a imagem no disco rígido do servidor
               if(imagemValida) fotografia.SaveAs(caminho);
               // redirecionar o utilizador para a página de INDEX
               return RedirectToAction("Index");
            }
            catch(Exception) {
               ModelState.AddModelError("", "Ocorreu um erro desconhecido. " +
                                            "Pedimos deculpa pela ocorrência.");
            }
         }
         // se cheguei aqui é pq alguma coisa correu mal...
         return View(agente);
      }

      // GET: Agentes/Edit/5
      [Authorize(Roles = "RecursosHumanos")]
      public ActionResult Edit(int? id) {
         if(id == null) {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }
         Agentes agentes = db.Agentes.Find(id);
         if(agentes == null) {
            return HttpNotFound();
         }
         return View(agentes);
      }

      // POST: Agentes/Edit/5
      // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
      // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
      [HttpPost]
      [ValidateAntiForgeryToken]
      [Authorize(Roles = "RecursosHumanos")]
      public ActionResult Edit([Bind(Include = "ID,Nome,Esquadra,Fotografia")] Agentes agentes) {
         if(ModelState.IsValid) {
            db.Entry(agentes).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
         }
         return View(agentes);
      }

      // GET: Agentes/Delete/5
      [Authorize(Roles = "RecursosHumanos")]
      public ActionResult Delete(int? id) {

         /// o ID é nulo se:
         ///   - há um erro no programa
         ///   - há um 'xico experto' a tentar a sua sorte
         /// redireciono o utilizador para a página de INDEX
         if(id == null) {
            return RedirectToAction("Index");
            //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         // procura os dados do Agente associado ao ID fornecido
         Agentes agente = db.Agentes.Find(id);

         /// o 'agente' é nulo se:
         ///   - há um erro no programa
         ///   - há um 'xico experto' a tentar a sua sorte
         /// redireciono o utilizador para a página de INDEX
         if(agente == null) {
            return RedirectToAction("Index");
            //return HttpNotFound();
         }

         /// para evitar 'trocas' maliciosa do 'agente'
         /// guardar o ID do agente, para futura comparação
         /// - num cookie cifrado
         /// - numa var. de sessão (não funciona no Asp .Net Core)
         /// - noutro recurso válido...
         Session["IdAgente"] = agente.ID;
         Session["acao"] = "Agentes/Delete";

         //  envia para a View os dados do Agente encontrado
         return View(agente);
      }

      // POST: Agentes/Delete/5
      [HttpPost, ActionName("Delete")]
      [ValidateAntiForgeryToken]
      [Authorize(Roles = "RecursosHumanos")]
      public ActionResult DeleteConfirmed(int? id) {

         /// o ID é nulo se:
         ///   - há um erro no programa
         ///   - há um 'xico experto' a tentar a sua sorte
         /// redireciono o utilizador para a página de INDEX
         if(id == null) {
            return RedirectToAction("Index");
            //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         // será que o ID do agente que aqui á fornecido
         // é o ID do agente apresentado no ecrã?
         if(id != (int)Session["IdAgente"] ||
            (string)Session["acao"] != "Agentes/Delete") {
            // redireciono o utilizador para a página de INDEX
            return RedirectToAction("Index");
         }


         // limpar o valor das Var. Sessão, porque não preciso mais delas
         Session["IdAgente"] = "";
         Session["acao"] = "";

         // procurar os dados do Agente a remover
         Agentes agente = db.Agentes.Find(id);
         //   Agentes agente = db.Agentes.Find((int)Session["IdAgente"]);

         /// o 'agente' é nulo se:
         ///   - há um erro no programa
         ///   - há um 'xico experto' a tentar a sua sorte
         /// redireciono o utilizador para a página de INDEX
         if(agente == null) {
            return RedirectToAction("Index");
            //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         try {
            // remove os dados do Agente do Modelo
            db.Agentes.Remove(agente);
            // consolida a remoção da BD
            db.SaveChanges();
         }
         catch(Exception) {
            // prepara mensagem de erro a ser enviada para o utilizador
            ModelState.AddModelError("", "ocorreu um erro com a remoção do agente " +
                                         agente.Nome +
                                         ". Provavelmente existem multas associadas" +
                                         " a esse agente.");
            // reenviar os dados do Agente para a View
            return View(agente);
         }

         // redireciona para a página INDEX
         return RedirectToAction("Index");
      }

      protected override void Dispose(bool disposing) {
         if(disposing) {
            db.Dispose();
         }
         base.Dispose(disposing);
      }
   }
}
