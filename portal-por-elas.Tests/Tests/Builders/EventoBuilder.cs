using portal_por_elas.Contracts.Request.Evento;
using portal_por_elas.Domain.Entities;

namespace portal_por_elas.Tests.Builders
{
    public class EventoBuilder
    {
        private string _nome = "Ação Social Por Elas - Itaquera";
        private DateOnly _dataEvento = new(2026, 12, 10);
        private TimeOnly _horaInicio = new(9, 0);
        private TimeOnly _horaFim = new(17, 0);

        public EventoBuilder ComNome(string nome)
        {
            _nome = nome;
            return this;
        }

        public EventoBuilder ComData(DateOnly data)
        {
            _dataEvento = data;
            return this;
        }

        public EventoBuilder ComHorario(TimeOnly inicio, TimeOnly fim)
        {
            _horaInicio = inicio;
            _horaFim = fim;
            return this;
        }

        public Evento Build() => new(_nome, _dataEvento, _horaInicio, _horaFim);

        public CriarEventoRequestDto BuildRequestDto() => new()
        {
            Nome = _nome,
            DataEvento = _dataEvento,
            HoraInicio = _horaInicio,
            HoraFim = _horaFim
        };
    }
}