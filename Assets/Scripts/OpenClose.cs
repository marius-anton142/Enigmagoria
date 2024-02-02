using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class OpenClose : MonoBehaviour
{
    private Animator animator;
    public bool isInRange;
    public KeyCode interactKey;
    public UnityEvent interactAction;
    //0 Loot closed
    //1 Loot open
    //2 Empty closed
    //3 Empty open
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }
    // Update is called once per frame
    void Update()
    {
        if(isInRange){
            if(Input.GetKeyDown(interactKey)){
                interactAction.Invoke();
            }
        }
    }

}
