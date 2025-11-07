using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Pistola : MonoBehaviour
{
    [Header("Disparo")]
    public GameObject ShootFx;
    public GameObject HitFx;
    public Transform firePoint;
    public LineRenderer line;
    public int damage = 25;

    [Header("Agarre")]
    public Transform attachPointLeft;
    public Transform attachPointRight;

    public Transform leftHandController;
    public Transform rightHandController;

    private XRGrabInteractable grabInteract;

    void Start()
    {
        grabInteract = GetComponent<XRGrabInteractable>();

        grabInteract.activated.AddListener(x => Disparando());

        if (line != null)
        {
            line.enabled = false;
            line.useWorldSpace = true;
            line.positionCount = 2;
            if (line.startWidth == 0f && line.endWidth == 0f)
            {
                line.startWidth = 0.01f;
                line.endWidth = 0.01f;
            }
        }
    }

    void Update()
    {
        if (grabInteract != null && !grabInteract.isSelected && leftHandController != null && rightHandController != null)
        {
            float distLeft = Vector3.Distance(transform.position, leftHandController.position);
            float distRight = Vector3.Distance(transform.position, rightHandController.position);

            if(distRight < distLeft && attachPointRight != null)
            {
                grabInteract.attachTransform = attachPointRight;
            }
            else if(attachPointLeft != null)
            {
                grabInteract.attachTransform = attachPointLeft;
            }
        }
    }

    public void Disparando()
    {
        StartCoroutine(Disparo());
    }

    private IEnumerator Disparo()
    {
        RaycastHit hit;
        bool hitInfo = Physics.Raycast(firePoint.position, firePoint.forward, out hit, 50f);
        Instantiate(ShootFx, firePoint.position, Quaternion.identity);

        if (hitInfo)
        {
            line.SetPosition(0, firePoint.position);
            line.SetPosition(1, hit.point);

            Instantiate(HitFx, hit.point, Quaternion.identity);
        }
        else
        {
            line.SetPosition(0, firePoint.position);
            line.SetPosition(1, firePoint.position + firePoint.forward * 20f);
        }

        line.enabled = true;
        yield return new WaitForSeconds(0.02f);
        line.enabled = false;
    }
}
