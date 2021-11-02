﻿public class RabbitQueueDTO
{
    public Arguments arguments { get; set; }
    public bool auto_delete { get; set; }
    public Backing_Queue_Status backing_queue_status { get; set; }
    public int consumer_capacity { get; set; }
    public int consumer_utilisation { get; set; }
    public int consumers { get; set; }
    public bool durable { get; set; }
    public Effective_Policy_Definition effective_policy_definition { get; set; }
    public bool exclusive { get; set; }
    public object exclusive_consumer_tag { get; set; }
    public object head_message_timestamp { get; set; }
    public string idle_since { get; set; }
    public int memory { get; set; }
    public int message_bytes { get; set; }
    public int message_bytes_paged_out { get; set; }
    public int message_bytes_persistent { get; set; }
    public int message_bytes_ram { get; set; }
    public int message_bytes_ready { get; set; }
    public int message_bytes_unacknowledged { get; set; }
    public Message_Stats message_stats { get; set; }
    public int messages { get; set; }
    public Messages_Details messages_details { get; set; }
    public int messages_paged_out { get; set; }
    public int messages_persistent { get; set; }
    public int messages_ram { get; set; }
    public int messages_ready { get; set; }
    public Messages_Ready_Details messages_ready_details { get; set; }
    public int messages_ready_ram { get; set; }
    public int messages_unacknowledged { get; set; }
    public Messages_Unacknowledged_Details messages_unacknowledged_details { get; set; }
    public int messages_unacknowledged_ram { get; set; }
    public string name { get; set; }
    public string node { get; set; }
    public object operator_policy { get; set; }
    public object policy { get; set; }
    public object recoverable_slaves { get; set; }
    public int reductions { get; set; }
    public Reductions_Details reductions_details { get; set; }
    public object single_active_consumer_tag { get; set; }
    public string state { get; set; }
    public string type { get; set; }
    public string vhost { get; set; }
}

public class Arguments
{
    public int xmaxpriority { get; set; }
    public string xqueuetype { get; set; }
}

public class Backing_Queue_Status
{
    public decimal avg_ack_egress_rate { get; set; }
    public decimal avg_ack_ingress_rate { get; set; }
    public decimal avg_egress_rate { get; set; }
    public decimal avg_ingress_rate { get; set; }
    public string[] delta { get; set; }
    public int len { get; set; }
    public string mode { get; set; }
    public int next_seq_id { get; set; }
    public Priority_Lengths priority_lengths { get; set; }
    public int q1 { get; set; }
    public int q2 { get; set; }
    public int q3 { get; set; }
    public int q4 { get; set; }
    public string target_ram_count { get; set; }
}

public class Priority_Lengths
{
    public int _0 { get; set; }
    public int _1 { get; set; }
    public int _2 { get; set; }
    public int _3 { get; set; }
    public int _4 { get; set; }
    public int _5 { get; set; }
}

public class Effective_Policy_Definition
{
}

public class Message_Stats
{
    public int ack { get; set; }
    public Ack_Details ack_details { get; set; }
    public int deliver { get; set; }
    public Deliver_Details deliver_details { get; set; }
    public int deliver_get { get; set; }
    public Deliver_Get_Details deliver_get_details { get; set; }
    public int deliver_no_ack { get; set; }
    public Deliver_No_Ack_Details deliver_no_ack_details { get; set; }
    public int get { get; set; }
    public Get_Details get_details { get; set; }
    public int get_empty { get; set; }
    public Get_Empty_Details get_empty_details { get; set; }
    public int get_no_ack { get; set; }
    public Get_No_Ack_Details get_no_ack_details { get; set; }
    public int publish { get; set; }
    public Publish_Details publish_details { get; set; }
    public int redeliver { get; set; }
    public Redeliver_Details redeliver_details { get; set; }
}

public class Ack_Details
{
    public decimal rate { get; set; }
}

public class Deliver_Details
{
    public decimal rate { get; set; }
}

public class Deliver_Get_Details
{
    public decimal rate { get; set; }
}

public class Deliver_No_Ack_Details
{
    public decimal rate { get; set; }
}

public class Get_Details
{
    public decimal rate { get; set; }
}

public class Get_Empty_Details
{
    public decimal rate { get; set; }
}

public class Get_No_Ack_Details
{
    public decimal rate { get; set; }
}

public class Publish_Details
{
    public decimal rate { get; set; }
}

public class Redeliver_Details
{
    public decimal rate { get; set; }
}

public class Messages_Details
{
    public decimal rate { get; set; }
}

public class Messages_Ready_Details
{
    public decimal rate { get; set; }
}

public class Messages_Unacknowledged_Details
{
    public decimal rate { get; set; }
}

public class Reductions_Details
{
    public decimal rate { get; set; }
}