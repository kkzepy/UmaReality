import sqlite3

def search_db(db_path, search_term):
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()

    # Get all table names
    cursor.execute("SELECT name FROM sqlite_master WHERE type='table';")
    tables = [row[0] for row in cursor.fetchall()]

    for table in tables:
        # Get all columns for the current table
        cursor.execute(f"PRAGMA table_info('{table}')")
        columns = [col[1] for col in cursor.fetchall()]

        for column in columns:
            query = f"SELECT * FROM \"{table}\" WHERE \"{column}\" LIKE ?"
            cursor.execute(query, (f'%{search_term}%',))
            results = cursor.fetchall()

            if results:
                print(f"Match found in Table: {table}, Column: {column}")
                for row in results:
                    pass#print(f"  -> {row}")

    conn.close()

search_db("E:\\Uma\\Persistent\\master\\master.mdb", '70003')