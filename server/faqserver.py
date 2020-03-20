#
# GENERAL COMMENTS: This class uses a TOTAL programming approach since we do not
# want to deal with exceptions in Python to ensure fast and easy programs
#


class FaqServer:
    """
    This class represents a connection to the "FAQ" server
    """

    def __init__(self):
        """
        During the initialization a connection is made with the server that hosts the
        "FAQs" and also provides the nlp tools with tasks


        CONCRETELY: I honestly have no idea at all how these things work, but I would
        assume that this is something that would be similar to the Java Sockets that
        we looked at in the networking course. So something like creating a connection
        to the server so that we can both ask stuff (has_request(), get_request()) 
        and send stuff (reply()).
        
        You can just hide all that networking in this class (in private methods and
        variables, which technically do not exist in Python, but are mimicked by 
        prepending the method name with two underscores, e.g. def __connect(ip_adress):
        or smt). We can then just pass a "FaqServer" object around and call methods
        on it, which gives us a very nice separation of server logic and model logic.
        
        As an exception, you are allowed to throw an exception here when the connection to the
        server failed, so that we now that we need to retry.
        """

    def has_request(self):
        """
        Check whether there is a request available at the server (such as "match
        questions" or "estimate offensiveness" ...)

        :return: True if and only if there is a request to be processed


        CONCRETELY: We use this function to actively wait on a request, but if you
        could implement a "wait-and-wake-up" method where the execution of the script
        just waits until there is a request and then wakes up that would be even better
        since active waiting will create unnecessary network traffic :)
        """

    def get_request(self):
        """
        Get the request to be processed as a JSON-object.


        :return: The request to be processed as a JSON-object.

        CONCRETELY: We expect three types of requests
        1. You want us to match a question:
        {
        "Action":"match_questions",
        "Primary":"XXX",
        "Questions":[
                    "AAA",
                    "BBB",
                    "CCC",
                    ...
                    ],
        "ID":"0123456789"
        }
        
        Here the 'Action' is the action to perform,
        the 'Primary' is the question to match to a list of questions
        the 'Questions' is a list of all the questions to compare with the 'Primary'
        
        2. You want us to estimate the offensiveness of a sentence:
        {
        "Action":"estimate_offensiveness",
        "Primary":"XXX",
        "ID":"0123456789"
        }
        
        Here the 'Action' is the action to perform,
        the 'Primary' is the question estimate the offensiveness of
        
        3. There is no action to perform:
        {
        "Action":"nothing",
        "ID":"0123456789"
        }
        
        The 'ID' is always used to include in the reply so that the server knows to
        which request the reply belongs. It is a simple unique integer, the format of
        which is decided by the server.
        """

    def reply(self, ans):
        """
        Forward the reply to the last action to the server. (e.g. the reply to the
        action that was described in the 'get_request()' method)

        :param ans: A JSON-object that contains the reply
        :return: None


        CONCRETELY: One of two types of replies will be given
        1. A reply to a "match_question":
        {
        "Primary":"XXX",
        "Match":"FFF",
        "Prob":0.XXX,
        "ID":"0123456789"
        }
        
        Here the 'Primary' is the sentence that was matched against,
        the 'Match' is the best match,
        the 'Prob' is the probability that the match is correct: a float between
        0 and 1 (1 = certainly equal, 0 = certainly unequal), a reasonable cut-off
        is 0.5
        
        2. A reply to an "estimate_offensiveness":
        {
        "Primary":"XXX",
        "Prob":0.XXX,
        "ID":"0123456789"
        }
        
        Here the 'Primary' is the sentence that was checked for offensiveness,
        the 'Prob' is the probability that the sentence is offensive: a float between
        0 and 1 (1 = certainly yes, 0 = certainly not), a reasonable cut-off
        is 0.7 à 0.8
        
        The 'ID' is always used to include in the reply so that the server knows to
        which request the reply belongs. It is a simple unique integer, the format of
        which is decided by the server. It corresponds to the 'ID' from a request from
        the 'get_request()' method.
        
        3. None values should just be ignored
        """
