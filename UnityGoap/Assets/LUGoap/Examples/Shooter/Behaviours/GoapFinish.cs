using LUGoap.Base;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoapFinish : MonoBehaviour
{
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
        if (_unit.health <= 0)
        {
            OnEndGame();
        }
    }

    public void OnEndGame()
    {
        _agent.ForceInterrupt(0);
        _unit.Explode();
        var currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
