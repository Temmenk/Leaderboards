using System;
using System.Collections;
using PlayFab;
using PlayFab.MultiplayerModels;
using TMPro;
using UnityEngine;

public class Matchmaker : MonoBehaviour
{
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject leaveQueueButton;
    [SerializeField] private TMP_Text queueStatusText;


    private string ticketId;
    private Coroutine pollticketCoroutine;
    private const string QueueName = "DefaultQueue";
    public void StartMatchMaking()
    {
        playButton.SetActive(false);
        queueStatusText.text = "submitting ticket";
        queueStatusText.gameObject.SetActive(true);

        PlayFabMultiplayerAPI.CreateMatchmakingTicket(
            new CreateMatchmakingTicketRequest
            {
                Creator = new MatchmakingPlayer
                {
                    Entity = new EntityKey
                    {
                        Id = PlayFabLogin.EntityId,
                        Type = "title_player_account"
                        
                    },
                    Attributes = new MatchmakingPlayerAttributes
                    {
                        DataObject = new{}
                    }                         
                        
                },

                GiveUpAfterSeconds = 120,

                QueueName = QueueName
                
            },
            OnMatchmakingTicketCreated,
            OnMatchmakingError
        );
    }

    private void OnMatchmakingError(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

    private void OnMatchmakingTicketCreated( CreateMatchmakingTicketResult result)
    {
        ticketId = result.TicketId;
        pollticketCoroutine = StartCoroutine(PollTicket());
        leaveQueueButton.SetActive(true);
        queueStatusText.text = "Ticket created";
    }

    

    private IEnumerator PollTicket()
    {
        while(true)
        {
            PlayFabMultiplayerAPI.GetMatchmakingTicket(
                new GetMatchmakingTicketRequest
                {
                    TicketId = ticketId,
                    QueueName = QueueName
                    
                },
                OnGetMatchmakingTicket,
                OnMatchmakingError
            );

            yield return new WaitForSeconds(6);
        }
    }

    private void OnGetMatchmakingTicket(GetMatchmakingTicketResult result)
    {
        queueStatusText.text = $"Status: {result.Status}";
        
        switch (result.Status)
        {
            case "Matched":
                StopCoroutine(pollticketCoroutine);
                StartMatch(result.MatchId);
                break;

            case "Canceled":
                break;
        }
    }
    private void StartMatch(string matchId)
    {
        queueStatusText.text = $"Starting match";
        PlayFabMultiplayerAPI.GetMatch(
            new GetMatchRequest
            {
             MatchId = matchId,
             QueueName = QueueName   
            },
            OnGetMatch,
            OnMatchmakingError
        );
    }

    private void OnGetMatch(GetMatchResult result)
    {
        queueStatusText.text = $"{result.Members[0].Entity.Id} vs {result.Members[1].Entity.Id}"; 
    }
}

