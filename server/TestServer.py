import unittest
from cluster import connector as cluster
import json
#Class of tests to test faqserver.py public methods that use a connection to the database server.



class TestServer(unittest.TestCase):
    #Initiate a server object
    @classmethod
    def setUpClass(cls):
        global currentServer
        currentServer = cluster.Connector()
        currentServer._base_request_uri = "https://clusterapidebug.azurewebsites.net/api/NLP"


    def test_has_task(self):
        #returns True or False.
        answer = currentServer.has_task()
        self.assertTrue((answer == True)|(answer == False))



    def test_get_next_task(self):
        answer = currentServer.get_next_task()
        # returns json object (-> in python this is a string or a dictionary).
        self.assertTrue((type(answer) == str)|(type(answer) == dict))
        #answer = json.loads(answer)                     #If answer would indeed be in the form of a string, it should be loaded as a dictionary

        # json object is of form  {
        # "Action":"match_questions",
        # "Primary":"XXX",
        # "Questions":[
        #             "AAA",
        #             "BBB",
        #             "CCC",
        #             ...
        #             ],
        # "ID":0123456789
        # }
        self.assertTrue(answer.keys() == ["Action","Primary","Questions","ID"])
        # 'Action is 'match_questions', 'estimate_offensiveness' or 'nothing'
        action_value = answer.getValue("Action")
        self.assertTrue(action_value == "match_questions"|action_value == "estimate_offensiveness"|action_value == "nothing")


    def test_reply(self):
        #server.reply()  has void return type so it cannot be tested from here (right ?)
        return True