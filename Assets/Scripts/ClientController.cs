using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;

public class ClientController : MonoBehaviour
{
    public NetworkDriver m_Driver;
    private NetworkConnection m_Connection;
    public bool Done;
    private NetworkEvent.Type cmd;

    private void Start()
    {
        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);

        var endpoint = NetworkEndPoint.LoopbackIpv4;
        endpoint.Port = 9000;

        m_Connection = m_Driver.Connect(endpoint);
    }

    private void OnDestroy()
    {
        m_Driver.Dispose();
    }

    private void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        //Não está conectado
        if (!m_Connection.IsCreated)
        {
            if (!Done)
            {
                return;
            }
        }
        
        while ((cmd = m_Driver.PopEvent(out m_Connection, out DataStreamReader reader)) != NetworkEvent.Type.Empty)
        {
            //Receiving data from client
            if (cmd == NetworkEvent.Type.Data)
            {
                int num = reader.ReadInt();
                Debug.Log("Numero recebido: " + num);

                total += num;

                var writer = m_Driver.BeginSend(NetworkPipeline.Null, m_Connections[i]);
                writer.WriteInt(total);
                m_Driver.EndSend(writer);
            }
            //Check if the message of client is a disconnection
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Cliente " + i + " desconectou");
                m_Connections[i] = default(NetworkConnection);
            }
        }
    }
}
