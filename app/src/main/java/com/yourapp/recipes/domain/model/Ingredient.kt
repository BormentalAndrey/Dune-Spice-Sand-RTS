package com.yourapp.recipes.domain.model

import java.util.UUID

data class Ingredient(
    val id: String = UUID.randomUUID().toString(),
    val name: String,
    val quantity: Float,
    val unit: String,
    val category: String = "other"
)
