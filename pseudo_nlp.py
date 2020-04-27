from cluster import connector
import json
import logging, sys
logging.basicConfig(stream=sys.stderr, level=logging.DEBUG)


def main():
    con = connector.Connector("ws://localhost:39160/api/NLP/WS")
    while True:
        try:
            task = con.get_next_task()
            print(task)
            sent = False
            while not sent:
                print(con._tasks_in_progress)
                reply = generate_response(task)
                print("Response: " + str(reply))
                try:
                    if reply != "" and len(reply) > 0:
                        con.reply(reply)
                        sent = True
                except TypeError as e:
                    print("Could not process input. TypeError: ")
                    print(e)
                except json.JSONDecodeError as e:
                    print("Could not process input. JSONDecodeError")
                    print(e)
        except KeyboardInterrupt:
            con.close()
            break


def generate_response(task):
    response = dict()
    try:
        response['msg_id'] = task['msg_id']
        if task['action'] == connector.Actions.ESTIMATE_OFFENSIVENESS.value:
            response['prob'] = 0
            response['sentence_id'] = task['sentence_id']
        elif task['action'] == connector.Actions.IS_NONSENSE.value:
            response['nonsense'] = False
            response['sentence_id'] = task['sentence_id']
        elif task['action'] == connector.Actions.MATCH_QUESTIONS.value:
            response['possible_matches'] = list()
            for question in task['compare_questions']:
                response['possible_matches'].append({'question_id': question['question_id'], 'prob': 1})
    except KeyError as ex:
        print(ex)
    return response

main()