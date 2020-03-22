from enum import Enum


class Actions(Enum):
    MATCH_QUESTIONS = "match_questions"
    ESTIMATE_OFFENSIVENESS = "estimate_offensiveness"


class Connector:
    def get_next_task(self, timeout=None):
        pass

    def has_task(self):
        return True

    def reply(self, response):
        pass