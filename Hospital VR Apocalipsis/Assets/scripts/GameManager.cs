using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Dictionary<ResiduoClasificable.TipoResiduo, ResiduoClasificable.ColorTarro> mapaClasificacion = new();
    public Dictionary<ResiduoClasificable.TipoResiduo, int> totalPorTipo = new();
    public Dictionary<ResiduoClasificable.TipoResiduo, int> depositadosPorTipo = new();
    public TextMeshProUGUI panelTexto;

    private void Awake()
    {
        Instance = this;
        mapaClasificacion = new Dictionary<ResiduoClasificable.TipoResiduo, ResiduoClasificable.ColorTarro> {
            { ResiduoClasificable.TipoResiduo.Biologico, ResiduoClasificable.ColorTarro.Rojo },
            { ResiduoClasificable.TipoResiduo.Quimico, ResiduoClasificable.ColorTarro.Amarillo },
            { ResiduoClasificable.TipoResiduo.Reciclable, ResiduoClasificable.ColorTarro.Verde },
            { ResiduoClasificable.TipoResiduo.PapelLimpio, ResiduoClasificable.ColorTarro.Azul },
            { ResiduoClasificable.TipoResiduo.Ordinario, ResiduoClasificable.ColorTarro.Naranja }
        };
    }

    public void RegistrarDeposito(ResiduoClasificable.TipoResiduo tipo)
    {
        if (!depositadosPorTipo.ContainsKey(tipo)) depositadosPorTipo[tipo] = 0;
        depositadosPorTipo[tipo]++;
        ActualizarPanel();
    }

    void ActualizarPanel()
    {
        string texto = "";
        foreach (var tipo in totalPorTipo.Keys)
        {
            int faltan = totalPorTipo[tipo] - depositadosPorTipo.GetValueOrDefault(tipo, 0);
            texto += $"{tipo}: {(faltan > 0 ? $"Faltan {faltan}" : "¡Completado!")}\n";
        }
        panelTexto.text = texto;
    }
}
