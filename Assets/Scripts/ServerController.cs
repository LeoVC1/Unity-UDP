using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;

public class ServerController : MonoBehaviour
{
    public NetworkDriver m_Driver;
    private NativeList<NetworkConnection> m_Connections;

    void Start()
    {
        m_Driver = NetworkDriver.Create();
        var endPoint = NetworkEndPoint.AnyIpv4;
        endPoint.Port = 9000;
        if(m_Driver.Bind(endPoint) != 0)
        {
            Debug.Log("Erro! Porta ja esta sendo usada!");
        }
        else
        {
            m_Driver.Listen();
        }

        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
    }

    private void OnDestroy()
    {
        m_Driver.Dispose();
        m_Connections.Dispose();
        
    }

    void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        ClearConnections();
        AcceptConnections();
        ReceiveMessages();
    }

    void ClearConnections()
    {
        for(int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated) //Check if connections still actives
            {
                m_Connections.RemoveAtSwapBack(i); //Clear inactive connection
                --i;
            }
        }
    }

    void AcceptConnections()
    {
        NetworkConnection connection;
        while((connection = m_Driver.Accept()) != default(NetworkConnection)) // Check new connection
        {
            m_Connections.Add(connection); //Aceppt new connection
            Debug.Log("Conexao Criada!");
        }
    }

    void ReceiveMessages()
    {
        int total = 0;
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections.IsCreated) //Check connection state
                continue;

            NetworkEvent.Type cmd;
            while((cmd = m_Driver.PopEventForConnection(m_Connections[i], out DataStreamReader stream)) != NetworkEvent.Type.Empty)
            {
                //Receiving data from client
                if(cmd == NetworkEvent.Type.Data) 
                {
                    int num = stream.ReadInt();
                    Debug.Log("Numero recebido: " + num);

                    total += num;

                    var writer = m_Driver.BeginSend(NetworkPipeline.Null, m_Connections[i]);
                    writer.WriteInt(total);
                    m_Driver.EndSend(writer);
                }
                //Check if the message of client is a disconnection
                else if(cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Cliente " + i + " desconectou");
                    m_Connections[i] = default(NetworkConnection);
                }
            }
        }
    }
}
