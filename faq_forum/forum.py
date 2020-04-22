from cluster import connector as cluster
from faq_forum.question_match import match
from faq_forum.auto_moderator import offensiveness, is_nonsense
import logging, sys
import traceback
#logging.basicConfig(stream=sys.stderr, level=logging.DEBUG)


def __get_match(question, question_set):
    """
    Find the best semantic match to question from the given question_set.

    Args:
        question: The question to match
        question_set: The questions to match to (given as a list of dicts {"question_id":XXX,"question":YYY})

    Returns: A tuple (p,best) consisting of the 'best' match from the given question_set and the probability,
    'p', that that 'best' match is semantically equal to the given question. (1 = equal, 0 = unequal,
    0 <= p <= 1). If the question_set was empty or None, (0.0, None) is returned

    """
    prob = 0.0
    best = None

    # Only loop over the questions in question_set if it is not None
    if question_set is not None:
        # Find the best match to the given question
        best = max(question_set, key=lambda x: match(question, x["question"]))
        prob = match(question, best["question"])

    # Right now we only store the best match, but we could respond with the X best
    # matches as well
    return \
        [
            {
                "question_id": best["question_id"],
                # Cast the numpy float32 to float
                "prob": float(prob)
            }
        ]


def __get_offensiveness(sentence):
    """
    Estimate the offensiveness of a sentence.
    Args:
        sentence: The sentence to estimate the offensiveness of

    Returns: the probability,'p', that that the given question is offensive. (1 = yes, 0 = no,
    0 <= p <= 1). 0.0 is returned if the given sentence was None.

    """
    if sentence is None:
        return 0.0
    return offensiveness(sentence)


def __unwrap_match_request(request):
    """
    Unwrap the given "match questions" request into a tuple of Python objects.

    Args:
        request: A JSON object describing a "match questions" request

    Returns: A tuple (question,question_set) where question is a string and question_set is an iterable
    collection of questions and their ids (given as dicts {"question_id":XXX,"question":YYY}).

    """
    # request = json.loads(request)
    question = request["question"]
    question_set = request["compare_questions"]

    return question, question_set


def __wrap_match_request(request, best_matches):
    """
    Wrap the given result of a "match questions" request in a JSON object

    Args:
        request: The request that was processed
        best_matches: A list of best matches and their probabilities, given as a list of
    {"question_id":XXX,"prob":YYY} dicts

    Returns: A JSON-like dict containing all the given information
    (as described on https://clusterdocs.azurewebsites.net/)

    """
    # Change float32s to floats
    for m in best_matches:
        # Cast the numpy float32 to float
        m["prob"] = float(m["prob"])

    ans = \
        {
            "question_id": request["question_id"],
            "possible_matches": best_matches,
            "msg_id": request["msg_id"],
            "question": request["question"]
        }

    return ans


def __unwrap_offensive_request(request):
    """
    Unwrap the given "estimate offensiveness" request into a string.

    Args:
        request: A JSON-like dict describing an "estimate offensiveness" request
        (as described on https://clusterdocs.azurewebsites.net/)

    Returns: A string that represents the sentence of which to estimate the offensiveness

    """
    # request = json.loads(request)
    sentence = request["sentence"]

    return sentence


def __wrap_offensive_request(request, prob):
    """
    Wrap the given result of an "estimate offensiveness" request in a JSON-like dict

    Args:
        request: The request that was processed
        prob: The probability that the question is offensive, a float

    Returns: A JSON-like dict containing all the given information
    (as described on https://clusterdocs.azurewebsites.net/)

    """
    ans = \
        {
            "sentence_id": request["sentence_id"],
            # Cast the numpy float32 to float
            "prob": float(prob),
            "msg_id": request["msg_id"],
            "sentence": request["sentence"]
        }

    return ans


def __unwrap_nonsense_request(request):
    """
    Unwrap the given "estimate nonsense" request into a string.

    Args:
        request: A JSON-like dict describing an "estimate nonsense" request
        (as described on https://clusterdocs.azurewebsites.net/)

    Returns: A string that represents the sentence of which to estimate the offensiveness

    """
    # request = json.loads(request)
    sentence = request["sentence"]

    return sentence


def __wrap_nonsense_request(request, is_nonsense):
    """
    Wrap the given result of an "estimate nonsense" request in a JSON-like dict

    Args:
        request: The request that was processed
        is_nonsense:    True if the question is nonsense
                        False if the question is not nonsense

    Returns: A JSON-like dict containing all the given information
    (as described on https://clusterdocs.azurewebsites.net/)

    """
    ans = \
        {
            "sentence_id": request["sentence_id"],
            "nonsense": is_nonsense,
            "msg_id": request["msg_id"],
            "sentence": request["sentence"]
        }

    return ans


def process(request):
    """
    Process the given request and store the reply in a JSON-like dict.
    The given request is not None, it is a JSON-like dict

    Args:
        request: A JSON-like dict describing the request
        (as described on https://clusterdocs.azurewebsites.net/)

    Returns: The reply to the given request

    """
    error = \
        {
            "error": "request was invalid"
        }

    # Process the request
    if request is None:
        ans = error
    else:
        # req_dict = json.loads(request)
        if "action" not in request:
            ans = error
        elif request["action"] == cluster.Actions.MATCH_QUESTIONS.value:
            inp = __unwrap_match_request(request)
            out = __get_match(inp[0], inp[1])
            ans = __wrap_match_request(request,
                                       out)
        elif request["action"] == cluster.Actions.ESTIMATE_OFFENSIVENESS.value:
            inp = __unwrap_offensive_request(request)
            out = __get_offensiveness(inp)
            ans = __wrap_offensive_request(request,
                                           out)
        elif request["action"] == cluster.Actions.IS_NONSENSE.value:
            inp = __unwrap_nonsense_request(request)
            # Should we try/catch here ?
            out = is_nonsense(inp)
            ans = __wrap_nonsense_request(request, out)
        else:
            ans = \
                {
                    "error": "the given request is not supported"
                }

    # return json.dumps(ans)
    return ans


def main():
    """
    Go into a while loop and wait for requests from Cluster.

    Returns: None

    """
    while True:
        try:
            # Connect to the server
            faq = cluster.Connector()
            break
        except Exception:
            # Retry connecting until it succeeds
            pass

    # Go to an infinite loop of processing requests of the FAQ forum
    while True:
        try:
            # Get the request
            print("::: waiting for request")
            request = faq.get_next_task(timeout=None)
            print("::: received request")

            # HOTFIX, needs to be fixed in the connector
            if "question" in request and "question_id" in request:
                request["sentence"] = request["question"]
                request["sentence_id"] = request["question_id"]

            # Process the request
            ans = process(request)

            while True:
                try:
                    # Answer to the request
                    print("::: sending answer")
                    print(request)
                    print(ans)
                    faq.reply(ans)
                    print("::: answer sent")
                    break
                except Exception:
                    traceback.print_exc()
                    print("::: could not send the answer")
                    # Retry sending the reply until it succeeds
                    pass
        except Exception:
            # Retry getting a request until it succeeds
            pass


def _test():
    req_1 = \
        {
            "action": cluster.Actions.MATCH_QUESTIONS.value,
            "question": "Where is the coffee machine?",
            "question_id": 123,
            "compare_questions": [
                {
                    "question_id": 111,
                    "question": "Where can I find the coffee machine?"
                },
                {
                    "question_id": 222,
                    "question": "When did the Titanic sink?"
                },
                {
                    "question_id": 333,
                    "question": "How can I use the coffee machine?"
                }
            ],
            "msg_id": 214
        }
    ans_1 = process(req_1)
    print(ans_1)

    req_2 = \
        {
            "action": cluster.Actions.ESTIMATE_OFFENSIVENESS.value,
            "question_id": 100,
            "question": "Charlie is a little bitch, haha, what a little shit :p",
            "msg_id": 345
        }
    ans_2 = process(req_2)
    print(ans_2)

    req_3 = \
        {
            "action": cluster.Actions.IS_NONSENSE.value,
            "question_id": 200,
            "question": "xmkjnezoinmkqzm. apeozfimkln. azefpqj wdsoimkalez.",
            "msg_id": 654
        }
    ans_3 = process(req_3)
    print(ans_3)


if __name__ == "__main__":
    # Remove the test() method in deployed scripts, it is purely used for debugging
    main()
