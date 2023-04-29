
using System.Collections.Generic;


[System.Serializable]
public class WorldState {
    public string key;
    public object value;
}

public class WorldStates
{
    public Dictionary<string, object> states;
    public WorldStates() {
        states = new Dictionary<string, object>();
    }

    public bool hasState(string key) {
        return states.ContainsKey(key);
    }

    void AddState(string key, object value) {
        states.Add(key, value);
    }

    public void SetState(string key, object value = null) {
        if (states.ContainsKey(key)) 
            states[key] = value;
        else 
            AddState(key, value);
        
    }

    public void RemoveState(string key) {
        if (states.ContainsKey(key))
            states.Remove(key);
    }

    public Dictionary<string, object> GetStates() {
        return states;
    }
}
