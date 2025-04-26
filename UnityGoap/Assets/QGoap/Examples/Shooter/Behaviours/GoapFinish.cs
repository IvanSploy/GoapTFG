using System.Threading.Tasks;
using QGoap.Base;
using UnityEngine;

public class GoapFinish : MonoBehaviour
{
    private bool _isDeath;
    private IAgent _agent;
    private GoapUnit _unit;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _agent = GetComponent<IAgent>();
        _unit = GetComponent<GoapUnit>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_isDeath) return;
        if (_unit.health <= 0)
        {
            _isDeath = true;
            OnEndGame();
        }
    }

    public async void OnEndGame()
    {
        _agent.ForceInterrupt(0);
        await Task.Delay(100);
        _unit.Explode();
    }
}
