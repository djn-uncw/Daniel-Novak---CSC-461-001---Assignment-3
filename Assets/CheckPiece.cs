using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OVR;
using Oculus.Interaction;
using Oculus.Interaction.Collections;

public class CheckPiece : MonoBehaviour
{
    // Reference variable to the corresponding skeleton piece in Skeleton Goal
    public Transform pieceTarget;

    // Reference variables to MeshRenderers within Skeleton Goal pieces
    // These will be used to turn on/off emission on said pieces.
    private MeshRenderer[] targetMeshRenderers;
    private List<Material> targetMaterialInstances = new List<Material>();

    // Variable to mark whether the current piece has already been placed   
    public bool isPlaced;

    // Variable to mark the sequence index of the piece
    public int sequenceIndex;

    // Shared tracker variable to track the current step of the sequence
    private static int currentSequenceIndex = 0;

    // Reference variables to components of the skeleton piece
    private Rigidbody rigidBody;
    private BoxCollider boxCollider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get the components of the skeleton piece
        rigidBody = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();

        // Grab the MeshRenderers of the corresponding Skeleton Goal piece
        if (pieceTarget != null)
        {
            targetMeshRenderers = pieceTarget.GetComponentsInChildren<MeshRenderer>(true);

            foreach (var rend in targetMeshRenderers)
            {
                if (rend != null)
                {
                    // Create unique material instances for each renderer so they don't overwrite globally
                    targetMaterialInstances.Add(rend.material);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

        // Check to see if the corresponding Skeleton Goal piece exists and it has at least one material instance
        if (pieceTarget != null && targetMaterialInstances.Count > 0)
        {
            // If the piece is the current piece in the sequence, activate the corresponding Skeleton Goal piece's emission.
            // Else, disable the emission of the corresponding Skeleton Goal piece.
            if (sequenceIndex == currentSequenceIndex)
            {
                foreach (var mat in targetMaterialInstances)
                {
                    if (mat != null)
                    {
                        mat.EnableKeyword("_EMISSION");
                    }
                }
            }
            else
            {
                foreach (var mat in targetMaterialInstances)
                {
                    if (mat != null)
                    {
                        mat.DisableKeyword("_EMISSION");
                    }
                }
            }
        }

        // If the current piece has already been placed, do nothing.
        if (isPlaced)
        {
            return;
        }

        // If the current piece is not the next piece in the sequence, do nothing.
        if (sequenceIndex != currentSequenceIndex)
        {
            return;
        }

        // Calculate the distance between the current skeleton piece and its corresponding target piece
        float distance = Vector3.Distance(transform.position, pieceTarget.position);

        // If the distance between the current piece and the target piece is less than 0.05f and the piece is not already placed, 
        // force the current piece to be unreleased from the player's hand, change its position/rotation to match the target piece,
        // freeze the position/rotation, and disable its grab. After all of that, mark the piece as placed so that these lines
        // of code run only once and also increment the current sequence index for the next piece.
        if (distance < 0.05f && !isPlaced)
        {
            GrabInteractable grabInteractable = GetComponentInChildren<GrabInteractable>();

            if (grabInteractable != null && grabInteractable.Interactors != null)
            {
                foreach (GrabInteractor interactor in grabInteractable.Interactors)
                {
                    interactor.Unselect();
                }
            }

            transform.SetPositionAndRotation(pieceTarget.position, pieceTarget.rotation);
            pieceTarget.gameObject.SetActive(false);

            rigidBody.constraints = RigidbodyConstraints.FreezeAll;
            boxCollider.enabled = false;

            isPlaced = true;

            currentSequenceIndex++;
        }

    }
}
