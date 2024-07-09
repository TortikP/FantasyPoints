using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.AI;
using Unity.Netcode;
using UnityEngine.Tilemaps;

public enum PlayerNumber
{
    neutral = -1,
    player1,
    player2,
    player3
}

public enum Faction
{
    neutral = -1,
    elementalist,
    druid,
    chaos
}

public enum BaseType
{
    MainBase,
    Crossroad,
    Fountain,
    Cannon
}

public class BaseLogic : NetworkBehaviour
{
    [SerializeField] private UnitLogic unitPrefab;
    [SerializeField] private NetworkVariable<int> amountOfUnits;
    [SerializeField] protected NetworkVariable<PlayerNumber> baseOwner;
    [SerializeField] public NetworkVariable<Faction> baseFaction;
    [SerializeField] protected BaseType baseType = BaseType.Crossroad;
    [SerializeField] protected float incomeDelay;
    [SerializeField] protected ulong baseIndex;
    [SerializeField] protected SpriteRenderer baseSprite;
    [SerializeField] private TMP_Text baseText;
    [SerializeField] private RuntimeAnimatorController[] factionAnimations;
    [SerializeField] protected float outlineSize = 1.0f;
    public IEnumerator enemyLogic;
    [HideInInspector] public bool isFrozen;
    float incomeTimer = 0;
    protected MaterialPropertyBlock propertyBlock;
    public override void OnNetworkSpawn()
    {
        baseIndex = NetworkObjectId;
        baseSprite.sortingOrder = Mathf.RoundToInt(-transform.position.y);
        amountOfUnits.OnValueChanged += AmountOfUnits_OnValueChanged;
        baseFaction.OnValueChanged += BaseFaction_OnValueChange;
        baseOwner.OnValueChanged += BaseOwner_OnValueChanged;
        if(propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }
       // propertyBlock.SetFloat("_Thickness", outlineSize);
       // baseSprite.SetPropertyBlock(propertyBlock);

    }
    private void AmountOfUnits_OnValueChanged(int previousValue, int currentValue)
    {
        if (IsClient)
        {
            baseText.text = currentValue.ToString();
        }
    }

    private void BaseFaction_OnValueChange(Faction previousValue, Faction currentValue)
    {
        if (IsClient)
        {
            switch (baseFaction.Value)
            {
                case Faction.elementalist:
                    GetComponent<Animator>().runtimeAnimatorController = factionAnimations[1];
                    break;
                case Faction.druid:
                    GetComponent<Animator>().runtimeAnimatorController = factionAnimations[2];
                    break;
                case Faction.chaos:
                    GetComponent<Animator>().runtimeAnimatorController = factionAnimations[3];
                    break;
                default:
                    GetComponent<Animator>().runtimeAnimatorController = factionAnimations[0];
                    break;
            }
            print(currentValue);
        }
    }

    private void BaseOwner_OnValueChanged(PlayerNumber previousValue,  PlayerNumber currentValue)
    {
        if (IsClient)
        {
            if (GameManager.Instance.myPlayerObject.playerId.Value == (int)currentValue)
            {
                baseSprite.material = GameManager.Instance.materials[1];
            }
            else
            {
                baseSprite.material = GameManager.Instance.materials[2];
            }
           // baseSprite.SetPropertyBlock(propertyBlock);
        }
    }

    public void DefineBase(PlayerController player)
    {
        
        if (player.playerId.Value == (int) baseOwner.Value)
        {
            SetBaseFactionServerRpc(player.playerFaction.Value);
            if (baseType == BaseType.MainBase)
            {
                player.AddMainBaseServerRpc();
            }
            if(GameMultiplayerManager.Instance.isSingleplayer && player.playerId.Value == (int) PlayerNumber.player2)
            {
                InitiateEnemyControl();
            }
        }

        if (player.isAI || !IsClient) return;

        if (GameManager.Instance.myPlayerObject.playerId.Value == (int)baseOwner.Value)
        {
            baseSprite.material = GameManager.Instance.materials[1];
        }
        else if (GameManager.Instance.myPlayerObject.playerId.Value != (int)baseOwner.Value && baseOwner.Value != PlayerNumber.neutral)
        {
            baseSprite.material = GameManager.Instance.materials[2];
        }
        else
        {
            baseSprite.material = GameManager.Instance.materials[0];
        }
    }

    public virtual void Update()
    {
        if (!IsServer)
        {
            return;
        }
        //Прирост юнитов на базе
        if (!GameManager.Instance.pause && !isFrozen)
        {
            if (incomeTimer > incomeDelay && incomeDelay != 0 && baseType != BaseType.Fountain)
            {
                if (amountOfUnits.Value < 100 && baseOwner.Value != PlayerNumber.neutral)
                {
                    SetAmountOfUnits(amountOfUnits.Value + 1);
                }

                incomeTimer = 0;
            }
            else
            {
                incomeTimer += Time.deltaTime;
            }
        }
    }

    public IEnumerator SendUnits(int unitNumber, ulong originId, ulong destinationId)
    {
        BaseLogic[] FromTo = new BaseLogic[2];
        FromTo[0] = NetworkManager.SpawnManager.SpawnedObjects[originId].GetComponent<BaseLogic>();
        FromTo[1] = NetworkManager.SpawnManager.SpawnedObjects[destinationId].GetComponent<BaseLogic>();
        PlayerNumber initialOwner = FromTo[0].GetBaseOwner();
        for (int i = 0; i < unitNumber; i++)
        {
            if (FromTo[0].GetBaseOwner() == initialOwner)
            {
                yield return new WaitUntil(() => !GameManager.Instance.pause && !isFrozen);
                SpawnUnitServerRpc(originId, destinationId);
                yield return new WaitForSeconds(0.5F);
            }
        }
    }

    public IEnumerator EnemyControl(float controlDelay)
    {
        //Локальная переменная для списка возможных баз для атаки
        List<BaseLogic> availableBases = new List<BaseLogic>();
        //Локальная переменная базы для атаки
        BaseLogic targetBase;
        print("Assumming control");
        //Не выполнять пока нет объекта противника
        yield return new WaitUntil(() => GameManager.Instance.enemyObject != null);
        //Выполнять пока id владельца базы равно id владельца объекта противника И AI включен
        //Выбери случайную базу для атаки
        while ((int) baseOwner.Value == GameManager.Instance.enemyObject.playerId.Value && GameManager.Instance.enemyObject.isAI)
        {
            //Пока не заморожена или игра не на паузе
            yield return new WaitUntil(() => !GameManager.Instance.pause && !isFrozen);
            //Задержка между исполнениями
            yield return new WaitForSeconds(controlDelay);
            //Случайное количество нужное для исполнения атаки
            int needUnits = Random.Range(15, 31);
            System.Random rand = new System.Random();
            if (amountOfUnits.Value > needUnits)
            {
                availableBases = GameManager.Instance.BaseList().Where(b => b.baseIndex != baseIndex).ToList();
                print(availableBases.Count);
                availableBases = availableBases.Where(b => b.baseOwner.Value != baseOwner.Value).ToList();
                print(availableBases.Count);
                //Выбор случайной базы не принадлежащей владельцу этой базы
                targetBase = availableBases.OrderBy(r => rand.Next()).FirstOrDefault();
                //Сколько отправить для атаки
                int amountSend = Random.Range(7, amountOfUnits.Value);
                //Если на базе больше юнитов чем на цели, то отправь вдвое больше чем на цели
                if (amountOfUnits.Value > targetBase.GetAmountOfUnits())
                {
                    amountSend = targetBase.GetAmountOfUnits() * 2;
                }
                //при отправлении больше юнитов чем есть, отправь столько, сколько есть-1
                if (amountSend > amountOfUnits.Value)
                {
                    amountSend = amountOfUnits.Value - 1;
                }
                amountOfUnits.Value = amountOfUnits.Value - amountSend;

                StartCoroutine(SendUnits(amountSend, NetworkObjectId, targetBase.NetworkObjectId));

                yield return new WaitForSeconds(amountSend * 1.0f);
            }
        }
    }
    public void SetAmountOfUnits(int amountOfUnits)
    {
        if (amountOfUnits > 0)
        {
            this.amountOfUnits.Value = amountOfUnits;
        }
        else { this.amountOfUnits.Value = 0; }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnUnitServerRpc(ulong originId, ulong destinationId)
    {
        UnitLogic unit = Instantiate(unitPrefab, NetworkManager.SpawnManager.SpawnedObjects[originId].transform.position, Quaternion.Euler(0, 0, 0));

        unit.GetComponent<NetworkObject>().Spawn(true);

        unit.InitializeUnit(originId, destinationId);
    }

    public void InitiateEnemyControl()
    {
        enemyLogic = EnemyControl(5f);
        StartCoroutine(enemyLogic);
    }
    [ServerRpc(RequireOwnership = false)]
    public void SetAmountOfUnitsServerRpc(int amountOfUnits)
    {
        if (amountOfUnits > 0)
        {
            this.amountOfUnits.Value = amountOfUnits;
        }
        else { this.amountOfUnits.Value = 0; }
    }

    public int GetAmountOfUnits()
    { 
        return amountOfUnits.Value; 
    }
    public void SetBaseFaction(Faction baseFaction)
    {
        this.baseFaction.Value = baseFaction;
    }
    [ServerRpc(RequireOwnership=false)]
    public void SetBaseFactionServerRpc(Faction baseFaction)
    {
        this.baseFaction.Value = baseFaction;
    }
    public Faction GetBaseFaction()
    {
        return baseFaction.Value;
    }

    public BaseType GetBaseType()
    {
        return baseType;
    }
    public virtual void SetBaseOwner(PlayerNumber playerNumber)
    {
        baseOwner.Value = playerNumber;

        if (GameMultiplayerManager.Instance.isSingleplayer)
        {
            if ((int) playerNumber != GameManager.Instance.enemyObject.playerId.Value && enemyLogic != null)
            {
                StopCoroutine(enemyLogic);
            }
            else if ((int) playerNumber == GameManager.Instance.enemyObject.playerId.Value)
            {
                InitiateEnemyControl();
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetBaseOwnerServerRpc(PlayerNumber playerNumber)
    {
        baseOwner.Value = playerNumber;
        if (GameMultiplayerManager.Instance.isSingleplayer)
        {
            if ((int)playerNumber != GameManager.Instance.enemyObject.playerId.Value && enemyLogic != null)
            {
                StopCoroutine(enemyLogic);
            }
            else if ((int) playerNumber == GameManager.Instance.enemyObject.playerId.Value)
            {
                InitiateEnemyControl();
            }
        }
    }

    public PlayerNumber GetBaseOwner()
    {
        return baseOwner.Value;
    }
    public ulong GetBaseIndex()
    {
        return baseIndex;
    }


    public float GetIncomeDelay()
    {
        return incomeDelay;
    }

    public void SetIncomeDelay(float incomeDelay)
    {
        this.incomeDelay = incomeDelay;
    }
}
