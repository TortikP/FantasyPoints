using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class PlayerController : NetworkBehaviour
{


    public Camera cam;
    public NetworkVariable<Faction> playerFaction;
    public NetworkVariable<int> playerId;
    public NetworkVariable<int> mainBases;
    public List<AbilitySO> playerAbilities;
    public NetworkVariable<int> firstAbilityId;
    public NetworkVariable<int> secondAbilityId;
    public NetworkVariable<bool> isReady;
    public AbilitySO currentAbility;
    public float resourceIncomeDelay;
    public int resourceCount = 0;
    public int playerManaFountains;
    public AbilityButtonLogic abilityButton;
    public bool isAI = false;


    private Vector3 previousHit;
    private ClientRpcParams clientRpcParams;
    private BaseLogic originBase;
    private bool isDrawing;
    private GameInterfaceLogic interfaceLogic;
    private CursorLogic playerCursor;
    private LineRenderer cursorLine;
    private int unitCount;
    private bool readyAbility;
    // Start is called before the first frame update

    public override void OnNetworkSpawn()
    {
        cam = Camera.main;
        interfaceLogic = FindObjectOfType<GameInterfaceLogic>();
        playerCursor = FindObjectOfType<CursorLogic>(true);
        cursorLine = FindObjectOfType<LineRenderer>(true);
        clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { OwnerClientId }
            }
        };
        if (IsServer)
        {
            mainBases.OnValueChanged += MainBases_OnValueChanged;
            isReady.Value = true;
        }
        if (IsOwner)
        {
            if (!isAI)
            {
                GameManager.Instance.myPlayerObject = this;

                StartCoroutine(ResourceIncome(resourceIncomeDelay));

                for (int i = 0; i < GameManager.Instance.abilitiesSOList.abilities.Length; i++)
                {
                    if (GameManager.Instance.abilitiesSOList.abilities[i].ability.abilityId == firstAbilityId.Value || GameManager.Instance.abilitiesSOList.abilities[i].ability.abilityId == secondAbilityId.Value)
                    {
                        playerAbilities.Add(GameManager.Instance.abilitiesSOList.abilities[i].ability);
                    }
                }

                if (playerAbilities.Count > 0)
                {
                    interfaceLogic.DefineAbilities(playerAbilities);
                }

                foreach(AbilityButtonLogic abilityButton in FindObjectsOfType<AbilityButtonLogic>())
                {
                    abilityButton.OnAbilityClick += Ability_OnAbilityClick;
                }

            }
            print("Player spawned: " + OwnerClientId);

            foreach (BaseLogic _base in FindObjectsOfType<BaseLogic>())
            {
                _base.DefineBase(this);
            }
            FindObjectOfType<AudioSource>().clip = GameManager.Instance.factionAudio[(int) playerFaction.Value];
            FindObjectOfType<AudioSource>().Play();
            GameManager.Instance.PlayersReadyServerRpc();

        }

    }

    private void Ability_OnAbilityClick(object sender, bool isEnoughResouces)
    {
        AbilityButtonLogic abilityButton = (AbilityButtonLogic) sender;
        if((currentAbility == null || currentAbility != abilityButton.ability) && isEnoughResouces)
        {
            //this.abilityButton = abilityButton;
            currentAbility = abilityButton.ability;
            readyAbility = true;
        }
        else
        {
            //this.abilityButton = null;
            currentAbility = null;
            readyAbility = false;
        }
        print(abilityButton.ability.name);
    }

    private void MainBases_OnValueChanged(int previousValue, int currentValue)
    {
        if(currentValue <= 0)
        {
            EndGameClientRpc();
        }
    }

    [ClientRpc]
    public void EndGameClientRpc()
    {
        StartCoroutine(GameManager.Instance.EndGame());
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddMainBaseServerRpc()
    {
        mainBases.Value++;
    }

    void Update()
    {
        Interact();
    }

    public void Interact()
    {
        //��������� ���� ���� ������ ������ ����������� ���������� ������� � �� �������������� �� � ���� �� �� �����
        if (!IsOwner || isAI || GameManager.Instance.pause) 
            return;
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)

        Touch touch = new Touch();
        bool inTouch;
        if (inTouch = Input.touchCount > 0)
        touch = Input.GetTouch(0);

#endif

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            
            if (currentAbility == null && !EventSystem.current.IsPointerOverGameObject())
            {
                Vector2 pos = Input.mousePosition;
                RaycastHit2D hit = Physics2D.Raycast(cam.ScreenToWorldPoint(pos), Vector3.forward);
                if (hit)
                {
                    if (hit.collider.TryGetComponent(out BaseLogic interactableBase) && hit.collider.CompareTag("Base"))
                    {
                        print(interactableBase.GetBaseIndex());
                        if (originBase != null)
                        {
                            if (originBase != interactableBase)
                            {
                                originBase.SetAmountOfUnitsServerRpc(originBase.GetAmountOfUnits() - unitCount);
                                originBase.StartCoroutine(originBase.SendUnits(unitCount, originBase.GetComponent<NetworkObject>().NetworkObjectId, 
                                    interactableBase.GetComponent<NetworkObject>().NetworkObjectId));
                            }
                            originBase = null;
                            unitCount = 0;
                            playerCursor.HideCursor();
                            interfaceLogic.unitsDrawn.gameObject.SetActive(false);
                        }
                        else if ((int)interactableBase.GetBaseOwner() == playerId.Value)
                        {
                            originBase = interactableBase;
                            playerCursor.ShowCursor();
                            cursorLine.gameObject.SetActive(true);
                            cursorLine.SetPosition(0, originBase.transform.position);
                            cursorLine.SetPosition(1, originBase.transform.position);
                            interfaceLogic.unitsDrawn.gameObject.SetActive(true);
                            isDrawing = true;
                        }
                    }
                }
            }
            if (
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            inTouch && touch.phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(touch.fingerId)
#else
            !EventSystem.current.IsPointerOverGameObject()
#endif
            )
            {
                readyAbility = false;
            }

        }
        else if (Input.GetKey(KeyCode.Mouse0))
        {
            if (isDrawing)
            {
                Vector2 cursorPos = cam.ScreenToWorldPoint(Input.mousePosition);
                cursorLine.SetPosition(1, playerCursor.transform.position);
                interfaceLogic.unitsDrawn.maximum = originBase.GetAmountOfUnits();
                if (Mathf.Abs((originBase.transform.position.x - cursorPos.x)) > Mathf.Abs((originBase.transform.position.y - cursorPos.y)))
                {
                    playerCursor.GetFillImage().fillAmount = Mathf.Abs((originBase.transform.position.x - cursorPos.x)) / 10;

                }
                else
                {
                    playerCursor.GetFillImage().fillAmount = Mathf.Abs((originBase.transform.position.y - cursorPos.y)) / 10;
                }
                unitCount = (int) (originBase.GetAmountOfUnits() * playerCursor.GetFillImage().fillAmount);
                playerCursor.underCursor.text = unitCount.ToString();
                interfaceLogic.unitsDrawn.current = unitCount;
            }
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {

            if (isDrawing)
            {
                cursorLine.gameObject.SetActive(false);
                isDrawing = false;
            }
            else if (currentAbility != null && !readyAbility)
            {
                if (currentAbility.abilityId != 3)
                {
                    FindObjectOfType<TilemapCollider2D>().enabled = true;
                }
                Vector2 pos = Input.mousePosition;
                RaycastHit2D hit = Physics2D.Raycast(cam.ScreenToWorldPoint(pos), Vector3.forward);
                if (hit)
                {
                    if (currentAbility != null)
                    {
                        Vector3 spawnPosition = new Vector3(cam.ScreenToWorldPoint(pos).x, cam.ScreenToWorldPoint(pos).y, 1);
                        SpawnAbilityServerRpc(currentAbility.abilityId, spawnPosition);
                    }
                }
                FindObjectOfType<TilemapCollider2D>().enabled = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            currentAbility = null;
            previousHit = default;
            FindObjectOfType<GameInterfaceLogic>().actionText.gameObject.SetActive(false);
            FindObjectOfType<AbilityButtonLogic>().cancelImg.gameObject.SetActive(false);
            FindObjectOfType<AbilityButtonLogic>().abilityImg.gameObject.SetActive(true);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void SpawnAbilityServerRpc(int abilityId, Vector3 abilitySpawnPosition, ServerRpcParams serverRpcParams = default)
    {
        print(serverRpcParams.Receive.SenderClientId);
        print(playerAbilities.Count);
        AbilitySO ability = GameManager.Instance.abilitiesSOList.abilities[abilityId].ability;

        print(ability.abilityPref);
        if (ability.hits == AbilityHits.OneHit)
        {
            if (ability.abilityId != 3)
            {
                GameObject abilityInstance = Instantiate(ability.abilityPref, abilitySpawnPosition, Quaternion.identity);
                abilityInstance.GetComponent<Ability>().playerOwner.Value = (PlayerNumber) playerId.Value;
                abilityInstance.GetComponent<NetworkObject>().SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);
                SendBackClientRpc(ability.abilityCost, clientRpcParams);
            }
            else
            {
                RaycastHit2D hit = Physics2D.Raycast(abilitySpawnPosition, Vector3.forward);
                if (hit)
                {
                    if (hit.collider.TryGetComponent(out BaseLogic baseLogic))
                    {
                        if((int) baseLogic.GetBaseOwner() == playerId.Value)
                        {
                            GameObject abilityInstance = Instantiate(ability.abilityPref, abilitySpawnPosition, Quaternion.identity);
                            abilityInstance.GetComponent<Ability>().playerOwner.Value = (PlayerNumber)playerId.Value;
                            abilityInstance.GetComponent<Rejuvinate>().targetBaseIndex = baseLogic.GetBaseIndex();
                            abilityInstance.GetComponent<NetworkObject>().SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);
                            SendBackClientRpc(ability.abilityCost, clientRpcParams);
                        }
                    }
                }
            }
        }
        else if(ability.hits == AbilityHits.TwoHits)
        {
            if (previousHit != default)
            {
                if (ability.abilityId == 5)
                {
                    GameObject abilityInstance = Instantiate(ability.abilityPref, Vector3.zero, Quaternion.identity);
                    abilityInstance.GetComponent<Ability>().playerOwner.Value = (PlayerNumber)playerId.Value;
                    abilityInstance.GetComponent<Teleport>().firstPoint = previousHit;
                    abilityInstance.GetComponent<Teleport>().lastPoint = abilitySpawnPosition;
                    abilityInstance.GetComponent<NetworkObject>().SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);
                    previousHit = default;
                    SendBackClientRpc(ability.abilityCost, clientRpcParams);
                }
            }
            else
            {
                previousHit = abilitySpawnPosition;
            }
        }
    }
    [ClientRpc]
    private void SendBackClientRpc(int abilityCost, ClientRpcParams clientRpcParams = default)
    {
        print("Check");
        currentAbility = null;
        resourceCount -= abilityCost;
        interfaceLogic.actionText.gameObject.SetActive(false);
        abilityButton = FindObjectsOfType<AbilityButtonLogic>().Where(a => a.IsSelected == true).FirstOrDefault();
        abilityButton.IsSelected = false;
        abilityButton.cancelImg.gameObject.SetActive(false);
        abilityButton.abilityImg.gameObject.SetActive(true);
    }

    public void SetAI(bool isAI)
    {
        this.isAI = isAI;
    }

    IEnumerator ResourceIncome(float resourceIncomeDelay)
    {
        while (true)
        {
            yield return new WaitUntil(() => !GameManager.Instance.pause);
            if (resourceCount < 100)
            {
                resourceCount++;
            }
            yield return new WaitForSeconds(resourceIncomeDelay / (1 + playerManaFountains));
        }
    }
}
