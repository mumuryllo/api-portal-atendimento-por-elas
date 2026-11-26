namespace portal_por_elas.Domain.Constants
{
    public class DataBaseConstants
    {
        public static class Tables
        {
            public const string Users = "users";
        }

        public static class Columns
        {
            public const string Id = "id";

            // User
            public const string Name = "name";
            public const string Email = "email";
            public const string PasswordHash = "password_hash";
            public const string Active = "active";
            public const string Role = "role";
            public const string CreatedAt = "created_at";
            public const string TemporaryPasswordExpiryTime = "temporary_password_expiry_time";
            public const string RefreshToken = "refresh_token";
            public const string RefreshTokenExpiryTime = "refresh_token_expiry_time";
        }

        public static class TabelaEvento
        {
            public const string NomeTabela = "eventos";
            public const string Id = "id";
            public const string Nome = "nome";
            public const string DataEvento = "data_evento";
            public const string HoraInicio = "hora_inicio";
            public const string HoraFim = "hora_fim";
            public const string Ativo = "ativo";
            public const string CriadoEm = "criado_em";
        }

        public static class TabelaServico
        {
            public const string NomeTabela = "servicos";
            public const string Id = "id";
            public const string Nome = "nome";
            public const string Ativo = "ativo";
        }

        public static class TabelaFilaAtendimento
        {
            public const string NomeTabela = "fila_atendimentos";
            public const string Id = "id";
            public const string EventoId = "evento_id";
            public const string NomeMulher = "nome_mulher";
            public const string Status = "status";
            public const string CriadoEm = "criado_em";
            public const string ChamadoEm = "chamado_em";
            public const string FinalizadoEm = "finalizado_em";
        }

        public static class TabelaFilaAtendimentoServico
        {
            public const string NomeTabela = "fila_atendimento_servicos";
            public const string FilaAtendimentoId = "fila_atendimento_id";
            public const string ServicoId = "servico_id";
        }

   
    }
}