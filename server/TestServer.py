import unittest
from cluster import connector as cluster
# Class of tests to test faqserver.py public methods that use a connection to the database server.


class TestServer(unittest.TestCase):
    # Initiate a server object
    @classmethod
    def setUpClass(cls):
        global currentServer
        global answer
        currentServer = cluster.Connector()
        currentServer._base_request_uri = "https://clusterapidebug.azurewebsites.net/api/NLP"
        answer = currentServer.get_next_task()

    def test_has_task(self):
        # returns True or False.
        answer = currentServer.has_task()
        self.assertTrue(answer or not answer)

    def test_get_next_task_correct_keys(self):
        # returns json-like dict
        self.assertTrue(type(answer) == dict)

        # The message has the right keys
        self.assertIn("action", answer.keys())
        self.assertIn("msg_id", answer.keys())
        self.assertIn("question_id", answer.keys())
        self.assertIn("question", answer.keys())

    def test_get_next_task_valid_action(self):
        # returns json-like dict
        self.assertTrue(type(answer) == dict)

        # The action is defined
        action = answer["action"]
        self.assertIn(action, [x.value for x in cluster.Actions])

    def test_get_next_task_valid_action_specific_keys(self):
        # returns json-like dict
        self.assertTrue(type(answer) == dict)

        # The action is defined
        action = answer["action"]
        if action == cluster.Actions.ESTIMATE_OFFENSIVENESS.value:
            # There is a compare_questions key
            self.assertIn("compare_questions", answer.keys())
            # The value has the correct format (e.g. [ {"question_id":...;"question":...} ])
            for tup in answer["compare_questions"]:
                self.assertTrue(type(tup) == dict)
                self.assertIn("question_id", tup.keys())
                self.assertIn("question", tup.keys())
    
    def test_reply(self):
        # server.reply()  has void return type so it cannot be tested from here (right ?)
        return True
