package com.yourapp.recipes.data.local.database.entity

import androidx.room.*

/**
 * Сущность коллекции рецептов (например, "Праздничные", "Для гостей").
 */
@Entity(
    tableName = "recipe_collections",
    indices = [Index(value = ["name"], unique = true)]
)
data class RecipeCollectionEntity(
    @PrimaryKey(autoGenerate = true)
    val id: Long = 0,
    
    @ColumnInfo(name = "name")
    val name: String,
    
    @ColumnInfo(name = "description")
    val description: String = "",
    
    @ColumnInfo(name = "icon")
    val icon: String = "folder"
)

/**
 * Связующая таблица многие-ко-многим между рецептами и коллекциями.
 */
@Entity(
    tableName = "recipe_collection_cross_ref",
    primaryKeys = ["recipe_id", "collection_id"],
    foreignKeys = [
        ForeignKey(
            entity = RecipeEntity::class,
            parentColumns = ["id"],
            childColumns = ["recipe_id"],
            onDelete = ForeignKey.CASCADE
        ),
        ForeignKey(
            entity = RecipeCollectionEntity::class,
            parentColumns = ["id"],
            childColumns = ["collection_id"],
            onDelete = ForeignKey.CASCADE
        )
    ],
    indices = [
        Index(value = ["collection_id"])
    ]
)
data class RecipeCollectionCrossRef(
    @ColumnInfo(name = "recipe_id")
    val recipeId: Long,
    
    @ColumnInfo(name = "collection_id")
    val collectionId: Long
)
