import sqlite3
from sqlite3 import Error


def get_connection(db_file):
    """
    Create a Connection object to the given file
    :param db_file: the database file to connect to
    :return: a Connection to the given database or None if
    something went wrong
    """
    try:
        return sqlite3.connect(db_file)
    except Error as e:
        print(e)
        return None


def create_table(connection, table_text):
    """
    Create a table described by the SQL command in table_text in the
    database given by the connection
    :param connection: a Connection to a database
    :param table_text: an SQL command to create a table
    :return: None
    """
    try:
        cursor = connection.cursor()
        cursor.execute(table_text)
    except Error as e:
        print(e)


if __name__ == '__main__':
    # Connect to the faq forum
    faq_forum = get_connection(r"C:\sqlite\db\faq_forum.db")

    # Create the answered and unanswered questions tables
    create_table(faq_forum,
                 """ CREATE TABLE IF NOT EXISTS answered (
                        id integer PRIMARY KEY,
                        question text NOT NULL,
                        answer text NOT NULL
                     ); """)
    create_table(faq_forum,
                 """ CREATE TABLE IF NOT EXISTS unanswered (
                        id integer PRIMARY KEY,
                        question text NOT NULL
                     ); """)
