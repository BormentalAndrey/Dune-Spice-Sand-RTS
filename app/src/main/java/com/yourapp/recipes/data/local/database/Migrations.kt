package com.yourapp.recipes.data.local.database

import androidx.room.migration.Migration
import androidx.sqlite.db.SupportSQLiteDatabase

/**
 * Миграции базы данных Room.
 * Обеспечивают безопасное обновление схемы БД при выходе новых версий приложения.
 */
object Migrations {
    
    /**
     * Миграция с версии 1 на версию 2.
     * Добавляет таблицу recipe_collections и связующую таблицу.
     */
    val MIGRATION_1_2 = object : Migration(1, 2) {
        override fun migrate(database: SupportSQLiteDatabase) {
            database.execSQL("""
                CREATE TABLE IF NOT EXISTS recipe_collections (
                    id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                    name TEXT NOT NULL,
                    description TEXT NOT NULL DEFAULT '',
                    icon TEXT NOT NULL DEFAULT 'folder'
                )
            """)
            
            database.execSQL("""
                CREATE UNIQUE INDEX IF NOT EXISTS index_recipe_collections_name 
                ON recipe_collections (name)
            """)
            
            database.execSQL("""
                CREATE TABLE IF NOT EXISTS recipe_collection_cross_ref (
                    recipe_id INTEGER NOT NULL,
                    collection_id INTEGER NOT NULL,
                    PRIMARY KEY (recipe_id, collection_id),
                    FOREIGN KEY (recipe_id) REFERENCES recipes(id) ON DELETE CASCADE,
                    FOREIGN KEY (collection_id) REFERENCES recipe_collections(id) ON DELETE CASCADE
                )
            """)
            
            database.execSQL("""
                CREATE INDEX IF NOT EXISTS index_recipe_collection_cross_ref_collection_id 
                ON recipe_collection_cross_ref (collection_id)
            """)
        }
    }
    
    val ALL_MIGRATIONS = arrayOf(MIGRATION_1_2)
}
