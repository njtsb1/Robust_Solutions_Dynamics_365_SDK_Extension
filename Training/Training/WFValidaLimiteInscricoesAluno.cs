using System;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;

namespace Treinamento
{
    public class WFValidaLimiteInscricoesAluno : CodeActivity
    {
        #region Parametros
        // recebe o usuário do contexto
        [Input("Usuario")]
        [ReferenceTarget("systemuser")]
        public InArgument<EntityReference> usuarioEntrada { get; set; }

        // recebe o contexto
        [Input("AlunoXCurso")]
        [ReferenceTarget("acdym_alunoxcurso")]
        public InArgument<EntityReference> RegistroContexto { get; set; }

        [Output("Saida")]
        public OutArgument<string> saida { get; set; }
        #endregion
        protected override void Execute(CodeActivityContext executionContext)
        {
            //Create the context            
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            ITracingService trace = executionContext.GetExtension<ITracingService>();

            // informação para o Log de Rastreamento de Plugin
            trace.Trace("acdym_alunoxcurso: " + context.PrimaryEntityId);

            // declaro variável com o Guid da entidade primária em uso
            Guid guidAlunoXCurso = context.PrimaryEntityId;

            // informação para o Log de Rastreamento de Plugin
            trace.Trace("guidAlunoXCurso: " + guidAlunoXCurso);

            String fetchAlunoXCursos = "<fetch distinct='false' mapping='logical' output-format='xml-platform' version='1.0'>";
            fetchAlunoXCursos += "<entity name='acdym_alunoxcurso' >";
            fetchAlunoXCursos += "<attribute name='acdym_alunoxcursoid' />";
            fetchAlunoXCursos += "<attribute name='acdym_namealunoxcurso' />";
            fetchAlunoXCursos += "<attribute name='acdym_emcurso' />";
            fetchAlunoXCursos += "<attribute name='createdon' />";
            fetchAlunoXCursos += "<attribute name='acdym_alunodio' />";
            fetchAlunoXCursos += "<order descending= 'false' attribute = 'acdym_namealunoxcurso' />";
            fetchAlunoXCursos += "<filter type= 'and' >";
            fetchAlunoXCursos += "<condition attribute = 'acdym_alunoxcursoid' value = '" + guidAlunoXCurso + "' uitype = 'acdym_alunoxcurso'  operator= 'eq' />";
            fetchAlunoXCursos += "</filter> ";
            fetchAlunoXCursos += "</entity>";
            fetchAlunoXCursos += "</fetch> ";
            trace.Trace("fetchAlunoXCursos: " + fetchAlunoXCursos);

            var entityAlunoXCursos = service.RetrieveMultiple(new FetchExpression(fetchAlunoXCursos));
            trace.Trace("entityAlunoXCursos: " + entityAlunoXCursos.Entities.Count);

            Guid guidAluno = Guid.Empty;
            foreach (var item in entityAlunoXCursos.Entities)
            {
                string nomeCurso = item.Attributes["acdym_namealunoxcurso"].ToString();
                trace.Trace("nomeCurso: " + nomeCurso);

                var entityAluno = ((EntityReference)item.Attributes["acdym_alunodio"]).Id;
                guidAluno = ((EntityReference)item.Attributes["acdym_alunodio"]).Id;
                trace.Trace("entityAluno: " + entityAluno);
            }

            String fetchAlunoXCursosQtde = "<fetch distinct='false' mapping ='logical' output-format ='xml-platform' version = '1.0'>";
            fetchAlunoXCursosQtde += "<entity name ='acdym_alunoxcurso'>";
            fetchAlunoXCursosQtde += "<attribute name= 'acdym_alunoxcursoid' />";
            fetchAlunoXCursosQtde += "<attribute name= 'acdym_namealunoxcurso' />";
            fetchAlunoXCursosQtde += "<attribute name= 'acdym_alunodio' />";
            fetchAlunoXCursosQtde += "<attribute name= 'createdon' />";
            fetchAlunoXCursosQtde += "<order descending= 'false' attribute = 'acdym_namealunoxcurso' />";
            fetchAlunoXCursosQtde += "<filter type= 'and' >";
            fetchAlunoXCursosQtde += "<condition attribute= 'acdym_alunodio' value = '" + guidAluno + "' operator= 'eq' />";
            fetchAlunoXCursosQtde += "</filter>";
            fetchAlunoXCursosQtde += "</entity>";
            fetchAlunoXCursosQtde += "</fetch>";
            trace.Trace("fetchAlunoXCursosQtde: " + fetchAlunoXCursosQtde);
            var entityAlunoXCursosQtde = service.RetrieveMultiple(new FetchExpression(fetchAlunoXCursosQtde));
            trace.Trace("entityAlunoXCursosQtde: " + entityAlunoXCursosQtde.Entities.Count);
            if (entityAlunoXCursosQtde.Entities.Count > 2)
            {
                saida.Set(executionContext, "Aluno excedeu limite de cursos ativos!");
                trace.Trace("Aluno excedeu limite de cursos ativos!");
                throw new InvalidPluginExecutionException("Aluno excedeu limite de cursos ativos!");
            }
        }
    }
}

